/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using log4net;
using Nini.Config;
using Nwc.XmlRpc;
using Mono.Addins;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Framework.Servers;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Services.Interfaces;



[assembly: Addin("DTLMoneyModule", "1.0")]
[assembly: AddinDependency("OpenSim.Region.Framework", OpenSim.VersionInfo.VersionNumber)]

namespace OpenSim.Forge.Currency
{
    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "DTLMoneyModule")]
    public class MoneyModule : IMoneyModule, ISharedRegionModule
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Where Stipends come from and Fees go to.
        /// </summary>
        private UUID EconomyBaseAccount = UUID.Zero;

        #region Constant numbers and members.

        // Constant memebers   
        private const int MONEYMODULE_REQUEST_TIMEOUT = 30000;
        private const int MONEYMODULE_INITIAL_BALANCE = 2000;
        public enum TransactionType : int
        {
            MONEY_TRANS_SYSTEMGENERATED = 0,
            MONEY_TRANS_REGIONMONEYREQUEST,
            MONEY_TRANS_GIFT,
            MONEY_TRANS_PURCHASE,
        }

        /// <summary>
        /// Where Stipends come from and Fees go to.
        /// </summary>
        // private UUID EconomyBaseAccount = UUID.Zero;

        private float EnergyEfficiency = 1f;
        // private ObjectPaid handerOnObjectPaid;
        private bool m_enabled = true;
        private bool m_sellEnabled = true;


        private bool m_enable_server = true;	// enable Money Server
        private string m_moneyServURL = string.Empty;
        private string m_userServIP = string.Empty;
        public BaseHttpServer HttpServer;

        /// <summary>
        /// Scenes by Region Handle
        /// </summary>
        private Dictionary<ulong, Scene> m_scenel = new Dictionary<ulong, Scene>();
        /// <summary>   
        /// To cache the balance data while the money server is not available.   
        /// </summary>   
        private Dictionary<UUID, int> m_moneyServer = new Dictionary<UUID, int>();

        // Events  


        // private int m_stipend = 1000;

        private int ObjectCount = 0;
        private int PriceEnergyUnit = 0;
        private int PriceGroupCreate = -1;
        private int PriceObjectClaim = 0;
        private float PriceObjectRent = 0f;
        private float PriceObjectScaleFactor = 10f;
        private int PriceParcelClaim = 0;
        private float PriceParcelClaimFactor = 1f;
        private int PriceParcelRent = 0;
        private int PricePublicObjectDecay = 0;
        private int PricePublicObjectDelete = 0;
        private int PriceRentLight = 0;
        private int PriceUpload = 0;
        private int TeleportMinPrice = 0;

        private float TeleportPriceExponent = 2f;

        #endregion

        #region IMoneyModule Members

#pragma warning disable 0067
        public event ObjectPaid OnObjectPaid;
#pragma warning restore 0067

        public int UploadCharge
        {
            get { return PriceUpload; }
        }

        public int GroupCreationCharge
        {
            get { return PriceGroupCreate; }
        }

        /// <summary>
        /// Called on startup so the module can be configured.
        /// </summary>
        /// <param name="config">Configuration source.</param>
        public void Initialise(IConfigSource config)
        {
            // Handle the parameters errors.
            if (config == null) return;
            ReadConfigAndPopulate(config);
        }

        public void Initialise(Scene scene, IConfigSource source)
        {
            Initialise(source);
            if (string.IsNullOrEmpty(m_moneyServURL)) m_enable_server = false;
            //
            AddRegion(scene);
        }

        public void AddRegion(Scene scene)
        {
            if (m_enabled)
            {
                scene.RegisterModuleInterface<IMoneyModule>(this);
                IHttpServer httpServer = MainServer.Instance;

                lock (m_scenel)
                {
                    if (m_scenel.Count == 0)
                    {
                        if (!string.IsNullOrEmpty(m_moneyServURL))
                        {
                            // XMLRPCHandler = scene;

                            // To use the following you need to add:
                            // -helperuri <ADDRESS TO HERE OR grid MONEY SERVER>
                            // to the command line parameters you use to start up your client
                            // This commonly looks like -helperuri http://127.0.0.1:9000/

                            httpServer.AddXmlRPCHandler("UpdateBalance", BalanceUpdateHandler);
                            httpServer.AddXmlRPCHandler("UserAlert", UserAlert);
                            httpServer.AddXmlRPCHandler("SendConfirmLink", SendConfirmLinkHandler);
                            httpServer.AddXmlRPCHandler("OnMoneyTransfered", OnMoneyTransferedHandler);

                            /*// Local Server..  enables functionality only.
                            httpServer.AddXmlRPCHandler("getCurrencyQuote", quote_func);
                            httpServer.AddXmlRPCHandler("buyCurrency", buy_func);
                            httpServer.AddXmlRPCHandler("preflightBuyLandPrep", preflightBuyLandPrep_func);
                            httpServer.AddXmlRPCHandler("buyLandPrep", landBuy_func);*/
                        }
                    }

                    if (m_scenel.ContainsKey(scene.RegionInfo.RegionHandle))
                    {
                        m_scenel[scene.RegionInfo.RegionHandle] = scene;
                    }
                    else
                    {
                        m_scenel.Add(scene.RegionInfo.RegionHandle, scene);
                    }
                }


                scene.EventManager.OnNewClient += OnNewClient;
                scene.EventManager.OnMoneyTransfer += MoneyTransferAction;
                scene.EventManager.OnClientClosed += ClientClosed;
                scene.EventManager.OnAvatarEnteringNewParcel += AvatarEnteringParcel;
                scene.EventManager.OnMakeChildAgent += MakeChildAgent;
                scene.EventManager.OnClientClosed += ClientLoggedOut;
                scene.EventManager.OnValidateLandBuy += ValidateLandBuy;
                scene.EventManager.OnLandBuy += ProcessLandBuy;
            }
        }

        public void RemoveRegion(Scene scene)
        {
        }

        public void RegionLoaded(Scene scene)
        {
        }


        // Please do not refactor these to be just one method
        // Existing implementations need the distinction
        //
        public void ApplyCharge(UUID agentID, int amount, MoneyTransactionType type, string extraData)
        {
            ApplyCharge(agentID, amount, type, extraData);
        }

        public void ApplyCharge(UUID agentID, int amount, MoneyTransactionType type)
        {
            ApplyCharge(agentID, amount, type);
        }

        public void ApplyUploadCharge(UUID agentID, int amount, string text)
        {
            ulong region = LocateSceneClientIn(agentID).RegionInfo.RegionHandle;
            PayMoneyCharge(agentID, amount, (int)MoneyTransactionType.UploadCharge, region, text);
        }

        public bool ObjectGiveMoney(UUID objectID, UUID fromID, UUID toID, int amount, UUID txn, out string result)
        {
            result = String.Empty;
            string objName = String.Empty;
            string avatarName = String.Empty;
            SceneObjectPart sceneObj = FindPrim(objectID);
            if (sceneObj != null)
            {
                objName = sceneObj.Name;
            }

            Scene scene = null;
            if (m_scenel.Count > 0)
            {
                scene = m_scenel[0];
            }
            if (scene != null)
            {
                UserAccount profile = scene.UserAccountService.GetUserAccount(scene.RegionInfo.ScopeID, toID);
                if (profile != null)
                {
                    avatarName = profile.FirstName + " " + profile.LastName;
                }
            }

            string description = String.Format("Object {0} pays {1}", objName, avatarName);
            bool give_result = TransferMoney(fromID, toID, amount, (int)MoneyTransactionType.ObjectPays, sceneObj.UUID, sceneObj.RegionHandle, description);

            return give_result;
        }

        public void PostInitialise()
        {
        }

        public void Close()
        {
        }

        public Type ReplaceableInterface
        {
            get { return typeof(IMoneyModule); }
        }

        public string Name
        {
            get { return "DTLMoneyModule"; }
        }

        #endregion

        /// <summary>
        /// Parse Configuration
        /// </summary>
        private void ReadConfigAndPopulate(IConfigSource config)
        {
            // we are enabled by default

            IConfig startupConfig = config.Configs["Startup"];

            if (startupConfig == null) // should not happen
                return;

            IConfig economyConfig = config.Configs["Economy"];

            // economymodule may be at startup or Economy (legacy)
            string mmodule = startupConfig.GetString("economymodule", "");
            if (String.IsNullOrEmpty(mmodule))
            {
                if (economyConfig != null)
                    mmodule = economyConfig.GetString("economymodule", "");
            }

            if (!String.IsNullOrEmpty(mmodule) && mmodule != Name)
            {
                // some other money module selected
                m_enabled = false;
                return;
            }

            if (economyConfig == null)
                return;

            m_userServIP = Util.GetHostFromDNS(economyConfig.GetString("user_server_url").Split(new char[] { '/', ':' })[3]).ToString();
            m_moneyServURL = economyConfig.GetString("CurrencyServer").ToString();
            // Check if the DTLMoneyModule is configured to load.
            if (economyConfig.GetString("EconomyModule").ToString() != "DTLMoneyModule")
            {
                return;
            }
            // Price
            PriceEnergyUnit = economyConfig.GetInt("PriceEnergyUnit", 100);
            PriceObjectClaim = economyConfig.GetInt("PriceObjectClaim", 10);
            PricePublicObjectDecay = economyConfig.GetInt("PricePublicObjectDecay", 4);
            PricePublicObjectDelete = economyConfig.GetInt("PricePublicObjectDelete", 4);
            PriceParcelClaim = economyConfig.GetInt("PriceParcelClaim", 1);
            PriceParcelClaimFactor = economyConfig.GetFloat("PriceParcelClaimFactor", 1f);
            PriceUpload = economyConfig.GetInt("PriceUpload", 0);
            PriceRentLight = economyConfig.GetInt("PriceRentLight", 5);
            TeleportMinPrice = economyConfig.GetInt("TeleportMinPrice", 2);
            TeleportPriceExponent = economyConfig.GetFloat("TeleportPriceExponent", 2f);
            EnergyEfficiency = economyConfig.GetFloat("EnergyEfficiency", 1);
            PriceObjectRent = economyConfig.GetFloat("PriceObjectRent", 1);
            PriceObjectScaleFactor = economyConfig.GetFloat("PriceObjectScaleFactor", 10);
            PriceParcelRent = economyConfig.GetInt("PriceParcelRent", 1);
            PriceGroupCreate = economyConfig.GetInt("PriceGroupCreate", 0);
            m_sellEnabled = economyConfig.GetBoolean("SellEnabled", true);
        }

        /*

        private void GetClientFunds(IClientAPI client)
        {
            CheckExistAndRefreshFunds(client.AgentId);
        }

        /// <summary>
        /// New Client Event Handler
        /// </summary>
        /// <param name="client"></param>
        private void OnNewClient(IClientAPI client)
        {
            GetClientFunds(client);

            // Subscribe to Money messages
            client.OnEconomyDataRequest += EconomyDataRequestHandler;
            client.OnMoneyBalanceRequest += SendMoneyBalance;
            client.OnRequestPayPrice += requestPayPrice;
            client.OnObjectBuy += ObjectBuy;
            client.OnLogout += ClientClosed;
        }

        /// <summary>
        /// Transfer money
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="Receiver"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        private bool doMoneyTransfer(UUID Sender, UUID Receiver, int amount, int transactiontype, string description)
        {
            bool result = true;

            return result;
        }


        /// <summary>
        /// Sends the the stored money balance to the client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="agentID"></param>
        /// <param name="SessionID"></param>
        /// <param name="TransactionID"></param>
        public void SendMoneyBalance(IClientAPI client, UUID agentID, UUID SessionID, UUID TransactionID)
        {
            if (client.AgentId == agentID && client.SessionId == SessionID)
            {
                int returnfunds = 0;

                try
                {
                    returnfunds = GetFundsForAgentID(agentID);
                }
                catch (Exception e)
                {
                    client.SendAlertMessage(e.Message + " ");
                }

                client.SendMoneyBalance(TransactionID, true, new byte[0], returnfunds, 0, UUID.Zero, false, UUID.Zero, false, 0, String.Empty);
            }
            else
            {
                client.SendAlertMessage("Unable to send your money balance to you!");
            }
        }*/

                            private SceneObjectPart FindPrim(UUID objectID)
        {
            lock (m_scenel)
            {
                foreach (Scene s in m_scenel.Values)
                {
                    SceneObjectPart part = s.GetSceneObjectPart(objectID);
                    if (part != null)
                    {
                        return part;
                    }
                }
            }
            return null;
        }

        private string resolveObjectName(UUID objectID)
        {
            SceneObjectPart part = FindPrim(objectID);
            if (part != null)
            {
                return part.Name;
            }
            return String.Empty;
        }/*

        private string resolveAgentName(UUID agentID)
        {
            // try avatar username surname
            Scene scene = GetRandomScene();
            UserAccount account = scene.UserAccountService.GetUserAccount(scene.RegionInfo.ScopeID, agentID);
            if (account != null)
            {
                string avatarname = account.FirstName + " " + account.LastName;
                return avatarname;
            }
            else
            {
                m_log.ErrorFormat(
                    "[MONEY]: Could not resolve user {0}",
                    agentID);
            }

            return String.Empty;
        }

        private void BalanceUpdate(UUID senderID, UUID receiverID, bool transactionresult, string description)
        {
            IClientAPI sender = LocateClientObject(senderID);
            IClientAPI receiver = LocateClientObject(receiverID);

            if (senderID != receiverID)
            {
                if (sender != null)
                {
                    sender.SendMoneyBalance(UUID.Random(), transactionresult, Utils.StringToBytes(description), GetFundsForAgentID(senderID), 0, UUID.Zero, false, UUID.Zero, false, 0, String.Empty);
                }

                if (receiver != null)
                {
                    receiver.SendMoneyBalance(UUID.Random(), transactionresult, Utils.StringToBytes(description), GetFundsForAgentID(receiverID), 0, UUID.Zero, false, UUID.Zero, false, 0, String.Empty);
                }
            }
        }

*/

        /// <summary>
        /// XMLRPC handler to send alert message and sound to client
        /// </summary>
        public XmlRpcResponse UserAlert(XmlRpcRequest request, IPEndPoint remoteClient)
        {
            bool ret = false;

            #region confirm the request and show the notice from money server.

            if (request.Params.Count > 0)
            {
                Hashtable requestParam = (Hashtable)request.Params[0];
                if (requestParam.Contains("clientUUID") &&
                    requestParam.Contains("clientSessionID") &&
                    requestParam.Contains("clientSecureSessionID"))
                {
                    UUID clientUUID = UUID.Zero;
                    UUID.TryParse((string)requestParam["clientUUID"], out clientUUID);
                    if (clientUUID != UUID.Zero)
                    {
                        IClientAPI client = LocateClientObject(clientUUID);
                        if (client != null &&
                            client.SessionId.ToString() == (string)requestParam["clientSessionID"] &&
                            client.SecureSessionId.ToString() == (string)requestParam["clientSecureSessionID"])
                        {
                            if (requestParam.Contains("Description"))
                            {
                                // Show the notice dialog with money server message.
                                GridInstantMessage gridMsg = new GridInstantMessage(null,
                                                                                     UUID.Zero,
                                                                                     "MonyServer",
                                                                                     new UUID(clientUUID.ToString()),
                                                                                     (byte)InstantMessageDialog.MessageFromAgent,
                                                                                     "Please clink the URI in IM window to confirm your purchase.",
                                                                                     false,
                                                                                     new Vector3());

                                client.SendInstantMessage(gridMsg);
                                ret = true;
                            }
                        }
                    }
                }
            }

            #endregion

            // Send the response to money server.
            XmlRpcResponse resp = new XmlRpcResponse();
            Hashtable paramTable = new Hashtable();
            paramTable["success"] = ret;

            resp.Value = paramTable;
            return resp;
        }

        #region Standalone box enablers only
        /*
                public XmlRpcResponse quote_func(XmlRpcRequest request, IPEndPoint remoteClient)
                {
                    // Hashtable requestData = (Hashtable) request.Params[0];
                    // UUID agentId = UUID.Zero;
                    int amount = 0;
                    Hashtable quoteResponse = new Hashtable();
                    XmlRpcResponse returnval = new XmlRpcResponse();


                    Hashtable currencyResponse = new Hashtable();
                    currencyResponse.Add("estimatedCost", 0);
                    currencyResponse.Add("currencyBuy", amount);

                    quoteResponse.Add("success", true);
                    quoteResponse.Add("currency", currencyResponse);
                    quoteResponse.Add("confirm", "asdfad9fj39ma9fj");

                    returnval.Value = quoteResponse;
                    return returnval;



                }

                public XmlRpcResponse buy_func(XmlRpcRequest request, IPEndPoint remoteClient)
                {
                    // Hashtable requestData = (Hashtable) request.Params[0];
                    // UUID agentId = UUID.Zero;
                    // int amount = 0;

                    XmlRpcResponse returnval = new XmlRpcResponse();
                    Hashtable returnresp = new Hashtable();
                    returnresp.Add("success", true);
                    returnval.Value = returnresp;
                    return returnval;
                }

                public XmlRpcResponse preflightBuyLandPrep_func(XmlRpcRequest request, IPEndPoint remoteClient)
                {
                    XmlRpcResponse ret = new XmlRpcResponse();
                    Hashtable retparam = new Hashtable();
                    Hashtable membershiplevels = new Hashtable();
                    ArrayList levels = new ArrayList();
                    Hashtable level = new Hashtable();
                    level.Add("id", "00000000-0000-0000-0000-000000000000");
                    level.Add("description", "some level");
                    levels.Add(level);
                    //membershiplevels.Add("levels",levels);

                    Hashtable landuse = new Hashtable();
                    landuse.Add("upgrade", false);
                    landuse.Add("action", "http://invaliddomaininvalid.com/");

                    Hashtable currency = new Hashtable();
                    currency.Add("estimatedCost", 0);

                    Hashtable membership = new Hashtable();
                    membershiplevels.Add("upgrade", false);
                    membershiplevels.Add("action", "http://invaliddomaininvalid.com/");
                    membershiplevels.Add("levels", membershiplevels);

                    retparam.Add("success", true);
                    retparam.Add("currency", currency);
                    retparam.Add("membership", membership);
                    retparam.Add("landuse", landuse);
                    retparam.Add("confirm", "asdfajsdkfjasdkfjalsdfjasdf");

                    ret.Value = retparam;

                    return ret;
                }

                public XmlRpcResponse landBuy_func(XmlRpcRequest request, IPEndPoint remoteClient)
                {
                    XmlRpcResponse ret = new XmlRpcResponse();
                    Hashtable retparam = new Hashtable();
                    // Hashtable requestData = (Hashtable) request.Params[0];

                    // UUID agentId = UUID.Zero;
                    // int amount = 0;

                    retparam.Add("success", true);
                    ret.Value = retparam;

                    return ret;
                }
        */
        #endregion

        #region local Fund Management
        /*
                /// <summary>
                /// Ensures that the agent accounting data is set up in this instance.
                /// </summary>
                /// <param name="agentID"></param>
                private void CheckExistAndRefreshFunds(UUID agentID)
                {

                }

                /// <summary>
                /// Gets the amount of Funds for an agent
                /// </summary>
                /// <param name="AgentID"></param>
                /// <returns></returns>
                private int GetFundsForAgentID(UUID AgentID)
                {
                    int returnfunds = 0;

                    return returnfunds;
                }

                // private void SetLocalFundsForAgentID(UUID AgentID, int amount)
                // {

                // }
        */
        #endregion

        #region Utility Helpers

        /// <summary>
        /// Locates a IClientAPI for the client specified
        /// </summary>
        /// <param name="AgentID"></param>
        /// <returns></returns>
        private IClientAPI LocateClientObject(UUID AgentID)
        {
            ScenePresence tPresence = null;
            IClientAPI rclient = null;

            lock (m_scenel)
            {
                foreach (Scene _scene in m_scenel.Values)
                {
                    tPresence = _scene.GetScenePresence(AgentID);
                    if (tPresence != null)
                    {
                        if (!tPresence.IsChildAgent)
                        {
                            rclient = tPresence.ControllingClient;
                        }
                    }
                    if (rclient != null)
                    {
                        return rclient;
                    }
                }
            }
            return null;
        }

        private Scene LocateSceneClientIn(UUID AgentId)
        {
            lock (m_scenel)
            {
                foreach (Scene _scene in m_scenel.Values)
                {
                    ScenePresence tPresence = _scene.GetScenePresence(AgentId);
                    if (tPresence != null)
                    {
                        if (!tPresence.IsChildAgent)
                        {
                            return _scene;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Utility function Gets a Random scene in the instance.  For when which scene exactly you're doing something with doesn't matter
        /// </summary>
        /// <returns></returns>
        public Scene GetRandomScene()
        {
            lock (m_scenel)
            {
                foreach (Scene rs in m_scenel.Values)
                    return rs;
            }
            return null;
        }

        /// <summary>
        /// Utility function to get a Scene by RegionID in a module
        /// </summary>
        /// <param name="RegionID"></param>
        /// <returns></returns>
        public Scene GetSceneByUUID(UUID RegionID)
        {
            lock (m_scenel)
            {
                foreach (Scene rs in m_scenel.Values)
                {
                    if (rs.RegionInfo.originRegionID == RegionID)
                    {
                        return rs;
                    }
                }
            }
            return null;
        }

        #endregion

        #region event Handlers

        private void RequestPayPrice(IClientAPI client, UUID objectID)
        {
            Scene scene = LocateSceneClientIn(client.AgentId);
            if (scene == null)
                return;

            SceneObjectPart task = scene.GetSceneObjectPart(objectID);
            if (task == null)
                return;
            SceneObjectGroup group = task.ParentGroup;
            SceneObjectPart root = group.RootPart;

            client.SendPayPrice(objectID, root.PayPrice);
        }

        /// <summary>
        /// When the client closes the connection we remove their accounting
        /// info from memory to free up resources.
        /// </summary>
        /// <param name="AgentID">UUID of agent</param>
        /// <param name="scene">Scene the agent was connected to.</param>
        /// <see cref="OpenSim.Region.Framework.Scenes.EventManager.ClientClosed"/>
        public void ClientClosed(UUID AgentID, Scene scene)
        {
            IClientAPI client = LocateClientObject(AgentID);
            if (client != null)
            {
                LogoffMoneyServer(client);
            }
        }

        private void ClientClosed(UUID AgentID)
        {
            IClientAPI client = LocateClientObject(AgentID);
            if (client != null)
            {
                // If the User is just transferred to another region. No need to logoff from money server.
                LogoffMoneyServer(client);
            }
        }
        /// <summary>
        /// Call this when the client disconnects.
        /// </summary>
        /// <param name="client"></param>
        private void ClientClosed(IClientAPI client)
        {
            if (client != null)
            {
                LogoffMoneyServer(client);
            }
        }

        /// <summary>
        /// Event called Economy Data Request handler.
        /// </summary>
        /// <param name="agentId"></param>
        private void EconomyDataRequestHandler(IClientAPI user)
        {
            if (user != null)
            {
                Scene s = (Scene)user.Scene;

                user.SendEconomyData(EnergyEfficiency, s.RegionInfo.ObjectCapacity, ObjectCount, PriceEnergyUnit, PriceGroupCreate,
                                     PriceObjectClaim, PriceObjectRent, PriceObjectScaleFactor, PriceParcelClaim, PriceParcelClaimFactor,
                                     PriceParcelRent, PricePublicObjectDecay, PricePublicObjectDelete, PriceRentLight, PriceUpload,
                                     TeleportMinPrice, TeleportPriceExponent);
            }
        }

        private void ValidateLandBuy(Object sender, EventManager.LandBuyArgs landBuyEvent)
        {
            IClientAPI senderClient = LocateClientObject(landBuyEvent.agentId);
            if (senderClient != null)
            {
                int balance = QueryBalanceFromMoneyServer(senderClient);
                if (balance >= landBuyEvent.parcelPrice)
                {
                    lock (landBuyEvent)
                    {
                        landBuyEvent.economyValidated = true;
                    }
                }
            }
        }

        private void ProcessLandBuy(Object sender, EventManager.LandBuyArgs landBuyEvent)
        {
            lock (landBuyEvent)
            {
                if (landBuyEvent.economyValidated == true &&
                    landBuyEvent.transactionID == 0)
                {
                    landBuyEvent.transactionID = Util.UnixTimeSinceEpoch();

                    ulong parcelID = (ulong)landBuyEvent.parcelLocalID;
                    UUID regionID = UUID.Zero;
                    if (sender is Scene) regionID = ((Scene)sender).RegionInfo.RegionID;

                    if (TransferMoney(landBuyEvent.agentId, landBuyEvent.parcelOwnerID,
                                      landBuyEvent.parcelPrice, (int)MoneyTransactionType.LandSale, regionID, parcelID, "Land Purchase"))
                    {
                        lock (landBuyEvent)
                        {
                            landBuyEvent.amountDebited = landBuyEvent.parcelPrice;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// THis method gets called when someone pays someone else as a gift.
        /// </summary>
        /// <param name="osender"></param>
        /// <param name="e"></param>
        private void MoneyTransferAction(Object sender, EventManager.MoneyTransferArgs moneyEvent)
        {
            // Check the money transaction is necessary.   
            if (moneyEvent.sender == moneyEvent.receiver)
            {
                return;
            }

            string description = string.Empty;
            UUID receiver = moneyEvent.receiver;
            if (moneyEvent.transactiontype == (int)MoneyTransactionType.PayObject)// Pay for the object.   
            {
                SceneObjectPart sceneObj = FindPrim(moneyEvent.receiver);
                if (sceneObj != null)
                {
                    receiver = sceneObj.OwnerID;
                }
                else
                {
                    return;
                }
            }

            // Before paying for the object, save the object local ID for current transaction.
            UUID objLocalID = UUID.Zero;
            ulong regionHandle = 0;
            if (moneyEvent.transactiontype == (int)MoneyTransactionType.PayObject)
            {
                // Notify the client.   
                if (sender is Scene)
                {
                    Scene scene = (Scene)sender;
                    regionHandle = scene.RegionInfo.RegionHandle;
                    objLocalID = scene.GetSceneObjectPart(moneyEvent.receiver).UUID;
                    m_log.Debug("Paying for object " + objLocalID);
                }
            }

            bool ret = TransferMoney(moneyEvent.sender,
                                     receiver,
                                     moneyEvent.amount,
                                     moneyEvent.transactiontype,
                                     objLocalID,
                                     regionHandle,
                                     description);
        }

        /// <summary>
        /// Event Handler for when a root agent becomes a child agent
        /// </summary>
        /// <param name="avatar"></param>
        private void MakeChildAgent(ScenePresence avatar)
        {

        }

        /// <summary>
        /// Event Handler for when the client logs out.
        /// </summary>
        /// <param name="AgentId"></param>
        private void ClientLoggedOut(UUID AgentId, Scene scene)
        {

        }

        /// <summary>
        /// Event Handler for when an Avatar enters one of the parcels in the simulator.
        /// </summary>
        /// <param name="avatar"></param>
        /// <param name="localLandID"></param>
        /// <param name="regionID"></param>
        private void AvatarEnteringParcel(ScenePresence avatar, int localLandID, UUID regionID)
        {
            ILandObject obj = avatar.Scene.LandChannel.GetLandObject(avatar.AbsolutePosition.X, avatar.AbsolutePosition.Y);
            if ((obj.LandData.Flags & (uint)ParcelFlags.AllowDamage) != 0)
            {
                avatar.Invulnerable = false;
            }
            else
            {
                avatar.Invulnerable = true;
            }
        }

        public int GetBalance(IClientAPI client)
        {
            return QueryBalanceFromMoneyServer(client);
        }

        public int GetBalance(UUID agentID)
        {
            IClientAPI client = (IClientAPI)LocateSceneClientIn(agentID);
            return QueryBalanceFromMoneyServer(client);
        }

        // Please do not refactor these to be just one method
        // Existing implementations need the distinction
        //
        public bool UploadCovered(IClientAPI client)
        {
            return true;
        }

        public bool UploadCovered(UUID agentID, int amount)
        {
            IClientAPI client = (IClientAPI)LocateSceneClientIn(agentID);
            int balance = QueryBalanceFromMoneyServer(client);
            if (balance < amount) return false;
            return true;
        }

        public bool AmountCovered(IClientAPI client, int amount)
        {
            return true;
        }

        public bool AmountCovered(UUID agentID, int amount)
        {
            IClientAPI client = (IClientAPI)LocateSceneClientIn(agentID);
            int balance = QueryBalanceFromMoneyServer(client);
            if (balance < amount) return false;
            return true;
        }

        #endregion

        public void ObjectBuy(IClientAPI remoteClient, UUID agentID,
                        UUID sessionID, UUID groupID, UUID categoryID,
                        uint localID, byte saleType, int salePrice)
        {
            // Handle the parameters error.   
            if (remoteClient == null || salePrice <= 0) return;

            // Get the balance from money server.   
            int balance = QueryBalanceFromMoneyServer(remoteClient);
            if (balance < salePrice)
            {
                remoteClient.SendAgentAlertMessage("Unable to buy now. You don't have sufficient funds.", false);
                return;
            }

            Scene scene = LocateSceneClientIn(remoteClient.AgentId);
            if (scene != null)
            {
                SceneObjectPart sceneObj = scene.GetSceneObjectPart(localID);
                if (sceneObj != null && sceneObj.SalePrice == salePrice)
                {
                    IBuySellModule mod = scene.RequestModuleInterface<IBuySellModule>();
                    bool ret = true;
                    if (mod != null)
                    {
                        ret = TransferMoney(remoteClient.AgentId, sceneObj.OwnerID, salePrice,
                                                (int)MoneyTransactionType.PayObject, sceneObj.UUID, sceneObj.RegionHandle, "Object Buy");
                    }
                    if (ret)
                    {
                        mod.BuyObject(remoteClient, categoryID, localID, saleType, salePrice);
                    }
                }
                else
                {
                    // Implmenting base sale data checking here so the default OpenSimulator implementation isn't useless
                    // combined with other implementations.  We're actually validating that the client is sending the data
                    // that it should.   In theory, the client should already know what to send here because it'll see it when it
                    // gets the object data.   If the data sent by the client doesn't match the object, the viewer probably has an
                    // old idea of what the object properties are.   Viewer developer Hazim informed us that the base module
                    // didn't check the client sent data against the object do any.   Since the base modules are the
                    // 'crowning glory' examples of good practice..

                    // Validate that the object exists in the scene the user is in
                    if (sceneObj == null)
                    {
                        remoteClient.SendAgentAlertMessage("Unable to buy now. The object was not found.", false);
                        return;
                    }

                    // Validate that the client sent the price that the object is being sold for
                    if (sceneObj.SalePrice != salePrice)
                    {
                        remoteClient.SendAgentAlertMessage("Cannot buy at this price. Buy Failed. If you continue to get this relog.", false);
                        return;
                    }
                }
            }
        }

        public void MoveMoney(UUID fromUser, UUID toUser, int amount, string text)
        {
        }

        public bool MoveMoney(UUID fromUser, UUID toUser, int amount, MoneyTransactionType type, string text)
        {
            return true;
        }

        #region MoneyModule XML-RPC Handler

        public XmlRpcResponse BalanceUpdateHandler(XmlRpcRequest request, IPEndPoint remoteClient)
        {
            bool ret = false;

            #region Update the balance from money server.

            if (request.Params.Count > 0)
            {
                Hashtable requestParam = (Hashtable)request.Params[0];
                if (requestParam.Contains("clientUUID") &&
                    requestParam.Contains("clientSessionID") &&
                    requestParam.Contains("clientSecureSessionID"))
                {
                    UUID clientUUID = UUID.Zero;
                    UUID.TryParse((string)requestParam["clientUUID"], out clientUUID);
                    if (clientUUID != UUID.Zero)
                    {
                        IClientAPI client = LocateClientObject(clientUUID);
                        if (client != null &&
                            client.SessionId.ToString() == (string)requestParam["clientSessionID"] &&
                            client.SecureSessionId.ToString() == (string)requestParam["clientSecureSessionID"])
                        {
                            if (requestParam.Contains("Balance"))
                            {
                                client.SendMoneyBalance(UUID.Random(),
                                                        true,
                                                        Utils.StringToBytes("Balance update event from money server"),
                                                        (int)requestParam["Balance"], 0, UUID.Zero, false, UUID.Zero, false, 0, String.Empty);
                                ret = true;
                            }
                        }
                    }
                }
            }

            #endregion

            // Send the response to money server.
            XmlRpcResponse resp = new XmlRpcResponse();
            Hashtable paramTable = new Hashtable();
            paramTable["success"] = ret;
            if (!ret)
            {
                m_log.ErrorFormat("[MONEY]: Cannot update client balance from MoneyServer.");
            }
            resp.Value = paramTable;

            return resp;
        }

        /// <summary>
        /// Event Handler for when an Avatar enters one of the parcels in the simulator.
        /// </summary>



        public XmlRpcResponse SendConfirmLinkHandler(XmlRpcRequest request, IPEndPoint remoteClient)
        {
            bool ret = false;

            #region confirm the request and send out confirm link.

            if (request.Params.Count > 0)
            {
                Hashtable requestParam = (Hashtable)request.Params[0];
                if (requestParam.Contains("clientUUID") &&
                    requestParam.Contains("clientSessionID") &&
                    requestParam.Contains("clientSecureSessionID"))
                {
                    UUID clientUUID = UUID.Zero;
                    UUID.TryParse((string)requestParam["clientUUID"], out clientUUID);
                    if (clientUUID != UUID.Zero)
                    {
                        IClientAPI client = LocateClientObject(clientUUID);
                        if (client != null &&
                            client.SessionId.ToString() == (string)requestParam["clientSessionID"] &&
                            client.SecureSessionId.ToString() == (string)requestParam["clientSecureSessionID"])
                        {
                            if (requestParam.Contains("URI"))
                            {
                                // Show the notice for user to confirm the link in IM.
                                GridInstantMessage gridMsg_notice = new GridInstantMessage(null,
                                                                                           UUID.Zero,
                                                                                           "MonyServer",
                                                                                           new UUID(clientUUID.ToString()),
                                                                                           (byte)InstantMessageDialog.MessageBox,
                                                                                           "Please clink the URI in IM window to confirm your purchase.",
                                                                                           false,
                                                                                           new Vector3());
                                client.SendInstantMessage(gridMsg_notice);

                                // Show the confirm link in IM window.
                                GridInstantMessage gridMsg_link = new GridInstantMessage(null,
                                                                                         UUID.Zero,
                                                                                         "MonyServer",
                                                                                         new UUID(clientUUID.ToString()),
                                                                                         (byte)InstantMessageDialog.MessageFromAgent,
                                                                                         (string)requestParam["URI"],
                                                                                         false,
                                                                                         new Vector3());
                                client.SendInstantMessage(gridMsg_link);

                                ret = true;
                            }
                        }
                    }
                }
            }

            #endregion

            // Send the response to money server.
            XmlRpcResponse resp = new XmlRpcResponse();
            Hashtable paramTable = new Hashtable();
            paramTable["success"] = ret;
            if (!ret)
            {
                m_log.ErrorFormat("[MONEY]: Cannot get or deliver the confirm link from MoneyServer.");
            }
            resp.Value = paramTable;

            return resp;
        }

        public XmlRpcResponse OnMoneyTransferedHandler(XmlRpcRequest request, IPEndPoint remoteClient)
        {
            bool ret = false;

            #region Confirm the transaction type and send out object paid event.

            if (request.Params.Count > 0)
            {
                Hashtable requestParam = (Hashtable)request.Params[0];
                if (requestParam.Contains("senderID") &&
                    requestParam.Contains("receiverID") &&
                    requestParam.Contains("senderSessionID") &&
                    requestParam.Contains("senderSecureSessionID"))
                {
                    UUID senderID = UUID.Zero;
                    UUID receiverID = UUID.Zero;
                    UUID.TryParse((string)requestParam["senderID"], out senderID);
                    UUID.TryParse((string)requestParam["receiverID"], out receiverID);
                    if (senderID != UUID.Zero)
                    {
                        IClientAPI client = LocateClientObject(senderID);
                        if (client != null &&
                            client.SessionId.ToString() == (string)requestParam["senderSessionID"] &&
                            client.SecureSessionId.ToString() == (string)requestParam["senderSecureSessionID"])
                        {
                            if (requestParam.Contains("transactionType") &&
                                requestParam.Contains("localID") &&
                                requestParam.Contains("amount"))
                            {
                                if ((int)requestParam["transactionType"] == 5008)// Pay for the object.
                                {
                                    // Notify the client.   
                                    ObjectPaid handlerOnObjectPaid = OnObjectPaid;
                                    if (handlerOnObjectPaid != null)
                                    {
                                        UUID localID = UUID.Zero;
                                        UUID.TryParse((string)requestParam["localID"], out localID);
                                        handlerOnObjectPaid(localID, senderID, (int)requestParam["amount"]);
                                        ret = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            // Send the response to money server.
            XmlRpcResponse resp = new XmlRpcResponse();
            Hashtable paramTable = new Hashtable();
            paramTable["success"] = ret;
            if (!ret)
            {
                m_log.ErrorFormat("[MONEY]: Transaction is failed. MoneyServer will rollback.");
            }
            resp.Value = paramTable;

            return resp;
        }

        // "GetBalance" RPC from Script
        public XmlRpcResponse GetBalanceHandler(XmlRpcRequest request, IPEndPoint remoteClient)
        {
            //m_log.InfoFormat("[MONEY]: GetBalanceHandler:");

            bool ret = false;
            int balance = -1;

            if (request.Params.Count > 0)
            {
                Hashtable requestParam = (Hashtable)request.Params[0];
                if (requestParam.Contains("clientUUID") &&
                    requestParam.Contains("clientSessionID") &&     // unable for Aurora-Sim
                    requestParam.Contains("clientSecureSessionID"))
                {
                    UUID clientUUID = UUID.Zero;
                    UUID.TryParse((string)requestParam["clientUUID"], out clientUUID);
                    if (clientUUID != UUID.Zero)
                    {
                        IClientAPI client = (IClientAPI)LocateSceneClientIn(clientUUID);
                        if (client != null &&
                            client.SessionId.ToString() == (string)requestParam["clientSessionID"] &&       // unable for Aurora-Sim
                            client.SecureSessionId.ToString() == (string)requestParam["clientSecureSessionID"])
                        {
                            balance = QueryBalanceFromMoneyServer(client);
                        }
                    }
                }
            }

            // Send the response to caller.
            if (balance < 0)
            {
                m_log.ErrorFormat("[MONEY]: GetBalanceHandler: GetBalance transaction is failed");
                ret = false;
            }

            XmlRpcResponse resp = new XmlRpcResponse();
            Hashtable paramTable = new Hashtable();
            paramTable["success"] = ret;
            paramTable["balance"] = balance;
            resp.Value = paramTable;

            return resp;
        }

        #endregion

        #region MoneyModule private help functions

        /// <summary>   
        /// Pay the money of charge.
        /// </summary>   
        /// <param name="amount">   
        /// The amount of money.   
        /// </param>   
        /// <returns>   
        /// return true, if successfully.   
        /// </returns>   
        private bool PayMoneyCharge(UUID sender, int amount, int type, ulong regionHandle, string description)
        {
            //m_log.InfoFormat("[MONEY]: PayMoneyCharge:");

            bool ret = false;
            IClientAPI senderClient = (IClientAPI)LocateSceneClientIn(sender);

            // Handle the illegal transaction.   
            if (senderClient == null) // receiverClient could be null.
            {
                m_log.InfoFormat("[MONEY]: PayMoneyCharge: Client {0} is not found", sender.ToString());
                return false;
            }

            if (QueryBalanceFromMoneyServer(senderClient) < amount)
            {
                m_log.InfoFormat("[MONEY]: PayMoneyCharge: No insufficient balance in client [{0}]", sender.ToString());
                return false;
            }

            #region Send transaction request to money server and parse the resultes.

            if (m_enable_server)
            {
                // Fill parameters for money transfer XML-RPC.   
                Hashtable paramTable = new Hashtable();
                paramTable["senderID"] = sender.ToString();
                paramTable["senderSessionID"] = senderClient.SessionId.ToString();
                paramTable["senderSecureSessionID"] = senderClient.SecureSessionId.ToString();
                paramTable["transactionType"] = type;
                paramTable["amount"] = amount;
                paramTable["regionHandle"] = regionHandle.ToString();
                paramTable["description"] = description;

                // Generate the request for transfer.   
                Hashtable resultTable = genericCurrencyXMLRPCRequest(paramTable, "PayMoneyCharge");

                // Handle the return values from Money Server.  
                if (resultTable != null && resultTable.Contains("success"))
                {
                    if ((bool)resultTable["success"] == true)
                    {
                        ret = true;
                    }
                }
                else m_log.ErrorFormat("[MONEY]: PayMoneyCharge: Can not pay money of charge request from [{0}]", sender.ToString());
            }
            //else m_log.ErrorFormat("[MONEY]: PayMoneyCharge: Money Server is not available!!");

            #endregion

            return ret;
        }

        /// <summary>   
        /// Transfer the money from one user to another. Need to notify money server to update.   
        /// </summary>   
        /// <param name="amount">   
        /// The amount of money.   
        /// </param>   
        /// <returns>   
        /// return true, if successfully.   
        /// </returns>   
        private bool TransferMoney(UUID sender,
                                   UUID receiver,
                                   int amount,
                                   int transactiontype,
                                   UUID localID,
                                   ulong regionHandle,
                                   string description)
        {
            bool ret = false;
            IClientAPI senderClient = LocateClientObject(sender);
            IClientAPI receiverClient = LocateClientObject(receiver);
            int senderBalance = -1;
            int receiverBalance = -1;

            Scene senderscene = (Scene)senderClient.Scene;
            Scene receiverscene = (Scene)receiverClient.Scene;

            // Handle the illegal transaction.   
            if (senderClient == null) // receiverClient could be null.
            {
                m_log.ErrorFormat("[MONEY]: Client {0} not found",
                                  ((senderClient == null) ? sender : receiver).ToString());
                return false;
            }

            if (QueryBalanceFromMoneyServer(senderClient) < amount)
            {
                m_log.ErrorFormat("[MONEY]: No insufficient balance in client [{0}].", sender.ToString());
                return false;
            }

            #region Send transaction request to money server and parse the resultes.

            if (!string.IsNullOrEmpty(m_moneyServURL))
            {
                // Fill parameters for money transfer XML-RPC.   
                Hashtable paramTable = new Hashtable();

                if (senderscene.UserManagementModule.IsLocalGridUser(sender))
                {
                    paramTable["senderUserServIP"] = m_userServIP;
                }
                else
                {
                    paramTable["senderUserServIP"] = Util.GetHostFromDNS(senderscene.UserManagementModule.GetUserHomeURL(sender).Split(new char[] { '/', ':' })[3]).ToString();
                }

                paramTable["senderID"] = sender.ToString();

                if (receiverscene.UserManagementModule.IsLocalGridUser(receiver))
                {
                    paramTable["receiverUserServIP"] = m_userServIP;
                }
                else
                {
                    paramTable["receiverUserServIP"] = Util.GetHostFromDNS(receiverscene.UserManagementModule.GetUserHomeURL(receiver).Split(new char[] { '/', ':' })[3]).ToString();
                }

                paramTable["receiverID"] = receiver.ToString();
                paramTable["senderSessionID"] = senderClient.SessionId.ToString();
                paramTable["senderSecureSessionID"] = senderClient.SecureSessionId.ToString();
                paramTable["transactionType"] = transactiontype;
                paramTable["localID"] = localID.ToString();
                paramTable["regionHandle"] = regionHandle.ToString();
                paramTable["amount"] = amount;
                paramTable["description"] = description;

                // Generate the request for transfer.   
                Hashtable resultTable = genericCurrencyXMLRPCRequest(paramTable, "TransferMoney");

                // Handle the return values from Money Server.  
                if (resultTable != null && resultTable.Contains("success"))
                {
                    if ((bool)resultTable["success"] == true)
                    {
                        m_log.DebugFormat("[MONEY]: Money transfer from client [{0}] to client [{1}] is done.",
                                          sender.ToString(),
                                          receiver.ToString());

                        ret = true;
                    }
                }
                else
                {
                    m_log.ErrorFormat("[MONEY]: Can not send money server transaction request from client [{0}].",
                                      sender.ToString(), receiver.ToString());
                    ret = false;
                }
            }
            else // If the money server is not available, save the balance into local.
            {
                if (m_moneyServer.ContainsKey(sender))
                {
                    if (!m_moneyServer.ContainsKey(receiver))
                    {
                        m_moneyServer.Add(receiver, MONEYMODULE_INITIAL_BALANCE);
                    }
                    m_moneyServer[sender] -= amount;
                    m_moneyServer[receiver] += amount;

                    senderBalance = m_moneyServer[sender];
                    receiverBalance = m_moneyServer[receiver];

                    ret = true;
                }
                else
                {
                    ret = false;
                }
            }

            #endregion

            return ret;
        }

        /// <summary>   
        /// Login the money server when the new client login.   
        /// </summary>   
        /// <param name="userID">   
        /// Indicate user ID of the new client.   
        /// </param>   
        /// <returns>   
        /// return true, if successfully.   
        /// </returns>   
        private bool LoginMoneyServer(IClientAPI client, out int balance)
        {
            bool ret = false;
            balance = 0;

            #region Send money server the client info for login.

            Scene scene = (Scene)client.Scene;
            string userName = string.Empty;

            if (!string.IsNullOrEmpty(m_moneyServURL))
            {
                // Get the username for the login user.
                if (client.Scene is Scene)
                {
                    if (scene != null)
                    {
                        userName = scene.UserManagementModule.GetUserName(client.AgentId);
                    }
                }

                // Login the Money Server.   
                Hashtable paramTable = new Hashtable();

                if (scene.UserManagementModule.IsLocalGridUser(client.AgentId))
                {
                    paramTable["userServIP"] = m_userServIP;
                }
                else
                {
                    paramTable["userServIP"] = Util.GetHostFromDNS(scene.UserManagementModule.GetUserHomeURL(client.AgentId).Split(new char[] { '/', ':' })[3]).ToString();
                }

                paramTable["openSimServIP"] = scene.RegionInfo.ServerURI.Replace(scene.RegionInfo.InternalEndPoint.Port.ToString(),
                                                                                 scene.RegionInfo.HttpPort.ToString());
                paramTable["userName"] = userName;
                paramTable["clientUUID"] = client.AgentId.ToString();
                paramTable["clientSessionID"] = client.SessionId.ToString();
                paramTable["clientSecureSessionID"] = client.SecureSessionId.ToString();

                // Generate the request for transfer.   
                Hashtable resultTable = genericCurrencyXMLRPCRequest(paramTable, "ClientLogin");

                // Handle the return result 
                if (resultTable != null && resultTable.Contains("success"))
                {
                    if ((bool)resultTable["success"] == true)
                    {
                        balance = (int)resultTable["clientBalance"];
                        m_log.InfoFormat("[MONEY]: Client [{0}] login Money Server {1}.",
                                         client.AgentId.ToString(), m_moneyServURL);
                        ret = true;
                    }
                }
                else
                {
                    m_log.ErrorFormat("[MONEY]: Unable to login Money Server {0} for client [{1}].",
                                      m_moneyServURL, client.AgentId.ToString());
                }
            }
            else // login to the local money server.
            {
                if (!m_moneyServer.ContainsKey(client.AgentId))
                {
                    m_moneyServer.Add(client.AgentId, MONEYMODULE_INITIAL_BALANCE);
                }
                balance = m_moneyServer[client.AgentId];

                ret = true;
            }

            #endregion

            return ret;
        }

        /// <summary>   
        /// Log off from the money server.   
        /// </summary>   
        /// <param name="userID">   
        /// Indicate user ID of the new client.   
        /// </param>   
        /// <returns>   
        /// return true, if successfully.   
        /// </returns>   
        private bool LogoffMoneyServer(IClientAPI client)
        {
            bool ret = false;

            Scene scene = (Scene)client.Scene;

            if (!string.IsNullOrEmpty(m_moneyServURL))
            {
                // Log off from the Money Server.   
                Hashtable paramTable = new Hashtable();

                if (scene.UserManagementModule.IsLocalGridUser(client.AgentId))
                {
                    paramTable["userServIP"] = m_userServIP;
                }
                else
                {
                    paramTable["userServIP"] = Util.GetHostFromDNS(scene.UserManagementModule.GetUserHomeURL(client.AgentId).Split(new char[] { '/', ':' })[3]).ToString();
                }

                paramTable["clientUUID"] = client.AgentId.ToString();
                paramTable["clientSessionID"] = client.SessionId.ToString();
                paramTable["clientSecureSessionID"] = client.SecureSessionId.ToString();

                // Generate the request for transfer.   
                Hashtable resultTable = genericCurrencyXMLRPCRequest(paramTable, "ClientLogout");
                // Handle the return result
                if (resultTable != null && resultTable.Contains("success"))
                {
                    if ((bool)resultTable["success"] == true)
                    {
                        ret = true;
                    }
                }
            }

            return ret;
        }


        /// <summary>   
        /// Generic XMLRPC client abstraction   
        /// </summary>   
        /// <param name="ReqParams">Hashtable containing parameters to the method</param>   
        /// <param name="method">Method to invoke</param>   
        /// <returns>Hashtable with success=>bool and other values</returns>   
        private Hashtable genericCurrencyXMLRPCRequest(Hashtable reqParams, string method)
        {
            // Handle the error in parameter list.   
            if (reqParams.Count <= 0 ||
                string.IsNullOrEmpty(method) ||
                string.IsNullOrEmpty(m_moneyServURL))
            {
                return null;
            }

            ArrayList arrayParams = new ArrayList();
            arrayParams.Add(reqParams);
            XmlRpcResponse moneyServResp = null;
            try
            {
                XmlRpcRequest moneyModuleReq = new XmlRpcRequest(method, arrayParams);
                moneyServResp = moneyModuleReq.Send(m_moneyServURL,
                                                    MONEYMODULE_REQUEST_TIMEOUT);
            }
            catch (Exception ex)
            {
                m_log.ErrorFormat(
                    "[MONEY]: Unable to connect to Money Server {0}.  Exception {1}",
                    m_moneyServURL, ex);

                Hashtable ErrorHash = new Hashtable();
                ErrorHash["success"] = false;
                ErrorHash["errorMessage"] = "Unable to manage your money at this time. Purchases may be unavailable";
                ErrorHash["errorURI"] = "";

                return ErrorHash;
            }

            if (moneyServResp.IsFault)
            {
                Hashtable ErrorHash = new Hashtable();
                ErrorHash["success"] = false;
                ErrorHash["errorMessage"] = "Unable to manage your money at this time. Purchases may be unavailable";
                ErrorHash["errorURI"] = "";

                return ErrorHash;
            }
            Hashtable moneyRespData = (Hashtable)moneyServResp.Value;

            return moneyRespData;
        }






        private int QueryBalanceFromMoneyServer(IClientAPI client)
        {
            int ret = -1;

            #region Send the request to get the balance from money server for cilent.

            Scene scene = (Scene)client.Scene;

            if (client != null)
            {
                if (!string.IsNullOrEmpty(m_moneyServURL))
                {
                    Hashtable paramTable = new Hashtable();

                    if (scene.UserManagementModule.IsLocalGridUser(client.AgentId))
                    {
                        paramTable["userServIP"] = m_userServIP;
                    }
                    else
                    {
                        paramTable["userServIP"] = Util.GetHostFromDNS(scene.UserManagementModule.GetUserHomeURL(client.AgentId).Split(new char[] { '/', ':' })[3]).ToString();
                    }

                    paramTable["clientUUID"] = client.AgentId.ToString();
                    paramTable["clientSessionID"] = client.SessionId.ToString();
                    paramTable["clientSecureSessionID"] = client.SecureSessionId.ToString();

                    // Generate the request for transfer.   
                    Hashtable resultTable = genericCurrencyXMLRPCRequest(paramTable, "GetBalance");

                    // Handle the return result
                    if (resultTable != null && resultTable.Contains("success"))
                    {
                        if ((bool)resultTable["success"] == true)
                        {
                            ret = (int)resultTable["clientBalance"];
                        }
                    }
                }
                else
                {
                    if (m_moneyServer.ContainsKey(client.AgentId))
                    {
                        ret = m_moneyServer[client.AgentId];
                    }
                }

                if (ret < 0)
                {
                    m_log.ErrorFormat("[MONEY]: Unable to query balance from Money Server {0} for client [{1}].",
                                      m_moneyServURL, client.AgentId.ToString());
                }
            }

            #endregion

            return ret;
        }



        /// <summary>   
        /// Sends the the stored money balance to the client   
        /// </summary>   
        /// <param name="client"></param>   
        /// <param name="agentID"></param>   
        /// <param name="SessionID"></param>   
        /// <param name="TransactionID"></param>   
        private void OnMoneyBalanceRequest(IClientAPI client, UUID agentID, UUID SessionID, UUID TransactionID)
        {
            if (client.AgentId == agentID && client.SessionId == SessionID)
            {
                int balance = -1;
                if (!string.IsNullOrEmpty(m_moneyServURL))
                {
                    balance = QueryBalanceFromMoneyServer(client);
                }
                else if (m_moneyServer.ContainsKey(agentID))
                {

                    balance = m_moneyServer[agentID];
                }

                if (balance < 0)
                {
                    client.SendAlertMessage("Fail to query the balance.");
                }
                else
                {
                    client.SendMoneyBalance(TransactionID, true, new byte[0], balance, 0, UUID.Zero, false, UUID.Zero, false, 0, String.Empty);
                }
            }
            else
            {
                client.SendAlertMessage("Unable to send your money balance.");
            }
        }

        private void OnNewClient(IClientAPI client)
        {
            int balance = 0;
            LoginMoneyServer(client, out balance);
            client.SendMoneyBalance(UUID.Zero, true, new byte[0], balance, 0, UUID.Zero, false, UUID.Zero, false, 0, String.Empty);

            // Subscribe to Money messages   
            client.OnEconomyDataRequest += EconomyDataRequestHandler;
            client.OnMoneyBalanceRequest += OnMoneyBalanceRequest;
            client.OnRequestPayPrice += RequestPayPrice;
            client.OnObjectBuy += ObjectBuy;
            client.OnLogout += ClientClosed;
        }



        #endregion



        public void ApplyUploadCharge(UUID agentID)
        {
            // Empty!
        }


        public void ApplyGroupCreationCharge(UUID agentID)
        {
            // Empty!
        }

        public bool GroupCreationCovered(IClientAPI client)
        {
            return true;
        }

    }

    public enum TransactionType : int
    {
     SystemGenerated = 0,
     RegionMoneyRequest = 1,
     Gift = 2,
     Purchase = 3
    }
}
