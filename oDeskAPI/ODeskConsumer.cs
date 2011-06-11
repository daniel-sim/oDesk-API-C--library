using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;

namespace oDeskAPI {
    /// <summary>
	/// A consumer capable of communicating with the oDesk API
	/// </summary>
	public static class ODeskConsumer {
		/// <summary>
		/// The Consumer to use for accessing oDesk API.
		/// </summary>
        public static readonly ServiceProviderDescription ServiceDescription = new ServiceProviderDescription
        {
            RequestTokenEndpoint = new MessageReceivingEndpoint(
                "https://www.odesk.com/api/auth/v1/oauth/token/request",
                HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.PostRequest),
            AccessTokenEndpoint = new MessageReceivingEndpoint(
                "https://www.odesk.com/api/auth/v1/oauth/token/access",
                HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.PostRequest),
            ProtocolVersion = ProtocolVersion.V10a,
            UserAuthorizationEndpoint = new MessageReceivingEndpoint(
                "https://www.odesk.com/services/api/auth",
                HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
            TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() }
        };

		/// <summary>
		/// The URI to get an organization once authorization is granted.
		/// </summary>
		private static readonly MessageReceivingEndpoint GetUserEndpoint
            = new MessageReceivingEndpoint("https://www.odesk.com/api/hr/v2/users/me.xml",
                HttpDeliveryMethods.GetRequest);

        private static readonly MessageReceivingEndpoint PostJobEndpoint
            = new MessageReceivingEndpoint("https://www.odesk.com/api/hr/v2/jobs.xml",
                HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

        private static readonly MessageReceivingEndpoint GetTeamsEndpoint
            = new MessageReceivingEndpoint("https://www.odesk.com/api/hr/v2/teams.xml",
                HttpDeliveryMethods.GetRequest);


		/// <summary>
		/// Requests authorization from ODesk to access API data
		/// </summary>
        public static void RequestAuthorization(WebConsumer consumer)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }
            var callback = Utility.GetCallbackUrlFromContext();
            consumer.Channel.Send(consumer.PrepareRequestUserAuthorization());
        }

		/// <summary>
		/// Gets the current oDesk user.
		/// </summary>
		/// <param name="consumer">The oDesk consumer.</param>
		/// <param name="accessToken">The access token previously retrieved.</param>
		/// <returns>An XML document returned by oDesk.</returns>
		public static XDocument GetUser(ConsumerBase consumer, string accessToken) {
			if (consumer == null)
				throw new ArgumentNullException("consumer");

			var request = consumer.PrepareAuthorizedRequest(GetUserEndpoint, accessToken);

			var response = consumer.Channel.WebRequestHandler.GetResponse(request);
			var body = response.GetResponseReader().ReadToEnd();
			var result = XDocument.Parse(body);
			return result;
		}

        /// <summary>
        /// Gets teams for the oDesk user.
        /// </summary>
        /// <param name="consumer">The oDesk consumer.</param>
        /// <param name="accessToken">The access token previously retrieved.</param>
        /// <returns>An XML document returned by oDesk.</returns>
        public static XDocument GetTeams(ConsumerBase consumer, string accessToken)
        {
            if (consumer == null)
                throw new ArgumentNullException("consumer");

            var request = consumer.PrepareAuthorizedRequest(GetTeamsEndpoint, accessToken);

            var response = consumer.Channel.WebRequestHandler.GetResponse(request);
            var body = response.GetResponseReader().ReadToEnd();
            var result = XDocument.Parse(body);
            return result;
        }

        /// <summary>
        ///  NOTE: Method does not currently work
        /// </summary>
        public static XDocument PostJob(ConsumerBase consumer, string accessToken, string apiKey, string sharedSecret,
            string title, string description, string buyerTeamReference)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }

            //List<NameValuePair> nameValuePairs = new ArrayList<NameValuePair>(2);
            
            //for (Map.Entry<String, String> entry : params.entrySet()) {
            //    String key = entry.getKey();
            //    String value = entry.getValue();
            //    nameValuePairs.add(new BasicNameValuePair(key, value));
            //}
            //httpPost.setEntity(new UrlEncodedFormEntity(nameValuePairs));
            

            var parameters = new Dictionary<string, string>()
                                 {
                                     {"title", title},
                                     {"job_type", "hourly"},
                                     {"description", description},
                                     {"buyer_team_reference", buyerTeamReference},
                                     {"visibility", "public"},
                                     {"duration", "16"},
                                     {"category", "Web Development"},
                                     {"subcategory", "Web Programming"}//,
                                     //{"api_key", apiKey}, Not sure these are required or not
                                     //{"api_token", accessToken}
                                 };
            var paramWrapper = new Dictionary<string, Dictionary<string, string>>()
                                   {
                                       {"job_data", parameters}
                                   };
            //parameters.Add("api_sig", CalcApiSig(parameters, sharedSecret));

            var parts = paramWrapper.Select(x => MultipartPostPart.CreateFormPart(x.Key, x.Value.ToString()));

            var request = consumer.PrepareAuthorizedRequest(PostJobEndpoint, accessToken, parts); //, extraData);

            var response = consumer.Channel.WebRequestHandler.GetResponse(request);
            var body = response.GetResponseReader().ReadToEnd();
            var result = XDocument.Parse(body);
            return result;
        }
	}
}
