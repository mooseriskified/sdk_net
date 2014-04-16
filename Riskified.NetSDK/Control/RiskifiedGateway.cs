﻿using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Riskified.NetSDK.Logging;
using Riskified.NetSDK.Model;
using Riskified.NetSDK.Exceptions;
using Riskified.NetSDK.Utils;

namespace Riskified.NetSDK.Control
{
    /// <summary>
    /// Main class to handle order creation and submittion to Riskified Servers
    /// </summary>
    public class RiskifiedGateway
    {
        private readonly Uri _riskifiedOrdersWebhookUrl;
        private readonly string _authToken;
        private readonly string _shopDomain;
        // TODO add test class
        
        public RiskifiedGateway(string riskifiedHostUrl, string authToken, string shopDomain)
        {
            _riskifiedOrdersWebhookUrl = HttpUtils.BuildUrl(riskifiedHostUrl,"/webhooks/merchant_order_created");
            // TODO make sure signature and domain are of valid structure
            _authToken = authToken;
            _shopDomain = shopDomain;
        }

        /// <summary>
        /// Validates the Order object fields
        /// Sends an order created/updated to Riskified Servers (without Submit for analysis)
        /// </summary>
        /// <param name="order">The Order to create or update</param>
        /// <returns>The order ID in riskified servers (for followup only - not used latter)</returns>
        /// <exception cref="OrderFieldBadFormatException">On bad format of the order (missing fields data or invalid data)</exception>
        /// <exception cref="RiskifiedTransactionException">On errors with the transaction itself (netwwork errors, bad response data)</exception>
        /// <exception cref="RiskifiedTransactionException">On errors with the transaction itself (netwwork errors, bad response data)</exception>
        public int CreateOrUpdateOrder(Order order)
        {
            return SendOrder(order, false);
        }

        /// <summary>
        /// Validates the Order object fields
        /// Sends an order to Riskified Servers and submits it for analysis
        /// </summary>
        /// <param name="order">The Order to submit</param>
        /// <returns>The order ID in riskified servers (for followup only - not used latter)</returns>
        /// <exception cref="OrderFieldBadFormatException">On bad format of the order (missing fields data or invalid data)</exception>
        /// <exception cref="RiskifiedTransactionException">On errors with the transaction itself (netwwork errors, bad response data)</exception>
        public int SubmitOrder(Order order)
        {
            return SendOrder(order, true);
        }

        /// <summary>
        /// Validates the Order object fields
        /// Sends the order to riskified server endpoint as configured in the ctor
        /// </summary>
        /// <param name="order">The order object to send</param>
        /// <param name="isSubmit">if the order should be submitted for inspection/analysis, flag should be true </param>
        /// <returns>The order ID in riskified servers (for followup only - not used latter)</returns>
        /// <exception cref="OrderFieldBadFormatException">On bad format of the order (missing fields data or invalid data)</exception>
        /// <exception cref="RiskifiedTransactionException">On errors with the transaction itself (netwwork errors, bad response data)</exception>
        private int SendOrder(Order order,bool isSubmit)
        {
            string jsonOrder;
            try
            {
                jsonOrder = JsonConvert.SerializeObject(order);
            }
            catch (Exception e)
            {
                throw new OrderFieldBadFormatException("The order could not be serialized to JSON: "+e.Message, e);
            }

            WebRequest request = HttpUtils.GeneratePostRequest(_riskifiedOrdersWebhookUrl, jsonOrder, _authToken,
                _shopDomain,HttpBodyType.JSON, isSubmit);
            WebResponse response;
            try
            {
                response = request.GetResponse();
            }
            catch (Exception e)
            {
                const string errorMsg = "There was an error sending order to server";
                LoggingServices.Error(errorMsg,e);
                throw new RiskifiedTransactionException("There was an error sending order to server",e);
            }

            var transactionResult = HttpUtils.ParseObjectFromJsonResponse < OrderTransactionResult>(response);

            if (transactionResult.IsSuccessful)
            {
                if (transactionResult.SuccessfulResult == null/* ||
                    (transactionResult.SuccessfulResult.Status != "submitted" &&
                     transactionResult.SuccessfulResult.Status != "created" &&
                     transactionResult.SuccessfulResult.Status != "updated" &&
                     transactionResult.SuccessfulResult.Status != "captured")*/)
                    throw new RiskifiedTransactionException(
                        "Error receiving valid response from riskified server - response wasn't in a known format");
            }
            else
            {
                //TODO handle case of unsuccessful tranaction of order
                throw new RiskifiedTransactionException("Case of failed response not implemented yet");
            }
            if (transactionResult.SuccessfulResult.Id != null)
                    return transactionResult.SuccessfulResult.Id.Value;

            string err = "Unknown Error occured - No Id received for a successful order";
            LoggingServices.Error(err);
            throw new RiskifiedTransactionException(err);

        }

        

    }

    
}
