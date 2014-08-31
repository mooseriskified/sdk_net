﻿using System;
using Riskified.SDK.Exceptions;
using Riskified.SDK.Model;
using Riskified.SDK.Utils;

namespace Riskified.SDK.Orders
{
    /// <summary>
    /// Main class to handle order creation and submittion to Riskified Servers
    /// </summary>
    public class OrdersGateway
    {
        private readonly string _riskifiedBaseWebhookUrl;
        private readonly string _authToken;
        private readonly string _shopDomain;
        
        /// <summary>
        /// Creates the mediator class used to send order data to Riskified
        /// </summary>
        /// <param name="env">The Riskified environment to send to</param>
        /// <param name="authToken">The merchant's auth token</param>
        /// <param name="shopDomain">The merchant's shop domain</param>
        public OrdersGateway(RiskifiedEnvironment env, string authToken, string shopDomain)
        {
            _riskifiedBaseWebhookUrl = EnvironmentsUrls.GetEnvUrl(env); 
            _authToken = authToken;
            _shopDomain = shopDomain;
        }

        /// <summary>
        /// Validates the Order object fields
        /// Sends a new order to Riskified Servers (without Submit for analysis)
        /// </summary>
        /// <param name="order">The Order to create</param>
        /// <returns>The order tranaction result containing status and order id  in riskified servers (for followup only - not used latter) in case of successful transfer</returns>
        /// <exception cref="OrderFieldBadFormatException">On bad format of the order (missing fields data or invalid data)</exception>
        /// <exception cref="RiskifiedTransactionException">On errors with the transaction itself (netwwork errors, bad response data)</exception>
        public OrderNotification Create(Order order)
        {            
            return SendOrder(order, HttpUtils.BuildUrl(_riskifiedBaseWebhookUrl, "/api/create"));
        }

        /// <summary>
        /// Validates the Order object fields
        /// Sends an updated order (already created) to Riskified Servers
        /// </summary>
        /// <param name="order">The Order to update</param>
        /// <returns>The order tranaction result containing status and order id  in riskified servers (for followup only - not used latter) in case of successful transfer</returns>
        /// <exception cref="OrderFieldBadFormatException">On bad format of the order (missing fields data or invalid data)</exception>
        /// <exception cref="RiskifiedTransactionException">On errors with the transaction itself (netwwork errors, bad response data)</exception>
        public OrderNotification Update(Order order)
        {
            return SendOrder(order, HttpUtils.BuildUrl(_riskifiedBaseWebhookUrl, "/api/update"));
        }

        /// <summary>
        /// Validates the Order object fields
        /// Sends an order to Riskified Servers and submits it for analysis
        /// </summary>
        /// <param name="order">The Order to submit</param>
        /// <returns>The order tranaction result containing status and order id  in riskified servers (for followup only - not used latter) in case of successful transfer</returns>
        /// <exception cref="OrderFieldBadFormatException">On bad format of the order (missing fields data or invalid data)</exception>
        /// <exception cref="RiskifiedTransactionException">On errors with the transaction itself (netwwork errors, bad response data)</exception>
        public OrderNotification Submit(Order order)
        {
            return SendOrder(order, HttpUtils.BuildUrl(_riskifiedBaseWebhookUrl, "/api/submit"));
        }

        /// <summary>
        /// Validates the cancellation data
        /// Sends a cancellation message for a specific order (id should already exist) to Riskified server for status and charge fees update
        /// </summary>
        /// <param name="orderCancellation"></param>
        /// <returns></returns>
        public OrderNotification Cancel(OrderCancellation orderCancellation)
        {
            return SendOrder(orderCancellation, HttpUtils.BuildUrl(_riskifiedBaseWebhookUrl, "/api/cancel"));
        }

        /// <summary>
        /// Validates the partial refunds data for an order
        /// Sends the partial refund data for an order to Riskified server for status and charge fees update
        /// </summary>
        /// <param name="orderPartialRefund"></param>
        /// <returns></returns>
        public OrderNotification PartlyRefund(OrderPartialRefund orderPartialRefund)
        {
            return SendOrder(orderPartialRefund, HttpUtils.BuildUrl(_riskifiedBaseWebhookUrl, "/api/refund"));
        }

        /// <summary>
        /// Validates the Order object fields
        /// Sends the order to riskified server endpoint as configured in the ctor
        /// </summary>
        /// <param name="order">The order object to send</param>
        /// <param name="riskifiedEndpointUrl">the endpoint to which the order should be sent</param>
        /// <returns>The order tranaction result containing status and order id  in riskified servers (for followup only - not used latter) in case of successful transfer</returns>
        /// <exception cref="OrderFieldBadFormatException">On bad format of the order (missing fields data or invalid data)</exception>
        /// <exception cref="RiskifiedTransactionException">On errors with the transaction itself (netwwork errors, bad response data)</exception>
        private OrderNotification SendOrder(AbstractOrder order, Uri riskifiedEndpointUrl)
        {
            var transactionResult = HttpUtils.JsonPostAndParseResponseToObject<OrderNotification, AbstractOrder>(riskifiedEndpointUrl, order, _authToken, _shopDomain);
            return transactionResult;
            
        }
    }

    
}