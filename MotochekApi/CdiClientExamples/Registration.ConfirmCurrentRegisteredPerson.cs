using System;
using System.Configuration;
using System.Linq;
using System.Net;


namespace CdiClient.CdiClientExamples.Registration
{
    public class ConfirmCurrentRegisteredPerson
    {
        #region Constructor

        public ConfirmCurrentRegisteredPerson()
        {
        }

        #endregion

        /// <summary>
        /// Execute the web method calls required to CDI to perform a Confirm Current Registered Person transaction
        /// </summary>
        /// 

        public static CdiServices.Vehicle.ArrayOfInquireStolenVehicleResponseResponseDetailResponseDetail getVehicleStolen(string rego)
        {
            Console.WriteLine();
            Console.WriteLine("Calling the CDI Confirm Current Registered Person transaction");
            Console.WriteLine();

            #region Local vars

            // These vars are used to retain values returned from CDI responses
            string cdiTokenId = null;               // The name of the CDIToken returned from CDI e.g. 'cdiToken'
            string cdiTokenValue = null;            // The value of the CDIToken returned from CDI e.g. '58be816a-8b84-4ab7-bdfd-20f2a946ff83'
            string cdiSessionToken = string.Empty;  // The value of the CDISessionToken returned from CDI e.g. '63ab802b-8b84-5ab7-bdgh-22f2a947ff82'
            bool cdiError = false;                  // Indicates that a CDI error was returned

            #endregion

            #region Step 1.0 - Call the CDI AuthenticateClient web method

            // Step 1.1 - Create AccessControl web service proxy
            Console.WriteLine("Calling the CDI AccessControl web service and AuthenticateClient web method");
            Console.WriteLine("- start -");
            CdiServices.AccessControl.AccessControl accessControlService = new CdiServices.AccessControl.AccessControl();

            // Step 1.2 - Create the request for the AuthenticateClient web method
            // The AuthenticateClient web method only requires an empty request to be input
            CdiServices.AccessControl.AuthenticateClientRequest authClientRequest = new CdiServices.AccessControl.AuthenticateClientRequest();
            authClientRequest.RequestBody = new object();

            // Step 1.3 - Create the soap header security

            // The UserNameToken is required by CDI to identify the client
            // CDI returns a CDIToken that uniquely identifies the client
            // The CDIToken must be submitted by the client with each subsequent request to CDI
            CdiServices.AccessControl.Security accessControlSecurity = new CdiServices.AccessControl.Security();
            accessControlSecurity.UserNameToken = new CdiServices.AccessControl.UserNameToken();
            accessControlSecurity.UserNameToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];     // The user name of the CDI client
            accessControlSecurity.UserNameToken.Password = ConfigurationManager.AppSettings["CdiPassword"];     // The password of the CDI client
            // Note: accessControlSecurity.UserNameToken.GroupName is optional

            // Set the soap security header
            accessControlService.SecurityValue = accessControlSecurity;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.SetTcpKeepAlive(false, 1000, 1000);
            // Step 1.4 - Call the AuthenticateClient web method
            CdiServices.AccessControl.AuthenticateClientResponse authClientResponse = null;
            authClientResponse = accessControlService.AuthenticateClient(authClientRequest);

            // Step 1.5 - Retain the CDIToken.Id and CDIToken.TokenValue returned from CDI
            // These need to be submitted with each subsequent web method call to CDI
            if (accessControlService.SecurityValue != null && accessControlService.SecurityValue.CDIToken != null && accessControlService.SecurityValue.CDIToken.Id != null)
            {
                cdiTokenId = accessControlService.SecurityValue.CDIToken.Id;
                Console.WriteLine("CDIToken.Id '{0}' returned from CDI", cdiTokenId);
            }

            if (accessControlService.SecurityValue != null && accessControlService.SecurityValue.CDIToken != null && accessControlService.SecurityValue.CDIToken.TokenValue != null)
            {
                cdiTokenValue = accessControlService.SecurityValue.CDIToken.TokenValue;
                Console.WriteLine("CDIToken.TokenValue {0} returned from CDI", cdiTokenValue);
            }

            // Step 1.6 - Retain the CDISessionToken returned from CDI
            // Thes needs to be submitted with each subsequent web method call to CDI
            if (accessControlSecurity != null && accessControlSecurity.CDISessionToken != null)
            {
                cdiSessionToken = accessControlService.SecurityValue.CDISessionToken;
                Console.WriteLine("CDISessionToken '{0}' returned from CDI", cdiSessionToken);
            }

            // Step 1.7 - Check for messages returned from CDI
            if (authClientResponse.ServiceHeader != null && authClientResponse.ServiceHeader.Status != null && authClientResponse.ServiceHeader.Status.Messages != null &&
                authClientResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.AccessControl.MessageType messageType in authClientResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 1.8 - Check for an error code returned from CDI
            if (authClientResponse != null && authClientResponse.ServiceHeader != null && authClientResponse.ServiceHeader.Status != null &&
                authClientResponse.ServiceHeader.Status.Code != null && authClientResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI AuthenticateClient web method completed ok");
            }
            else
            {
                cdiError = true;
                Console.WriteLine("CDI AuthenticateClient web method did not complete ok");
            }

            Console.WriteLine("- end -");
            Console.WriteLine();

            #endregion

            #region testVehicle
            CdiServices.Vehicle.Vehicle vehicle = new CdiServices.Vehicle.Vehicle();


            CdiServices.Vehicle.InquireStolenVehicleRequest inquireVehicleDetailsRequest = new CdiServices.Vehicle.InquireStolenVehicleRequest();
            inquireVehicleDetailsRequest.RequestBody = new CdiServices.Vehicle.VehicleOrPlateType[1];
            inquireVehicleDetailsRequest.RequestBody[0] = new CdiServices.Vehicle.VehicleOrPlateType();

            CdiServices.Vehicle.BasicPlateType basicPlate1 = new CdiClient.CdiServices.Vehicle.BasicPlateType();
            basicPlate1.PlateNumber = rego;
            inquireVehicleDetailsRequest.RequestBody[0].Item = basicPlate1;      // Add the plate to the request
                                                                                 // Step 2.3 - Create the soap header security
            CdiServices.Vehicle.Security registrationSecurity2 = new CdiServices.Vehicle.Security();

            // Set the SecurityTokenReference.Id previously returned by CDI
            // This holds the name/id of the CDI token e.g. 'cdiToken'
            registrationSecurity2.SecurityTokenReference = new CdiServices.Vehicle.SecurityTokenReference();
            registrationSecurity2.SecurityTokenReference.Id = cdiTokenId;

            // Set the CDIToken previously returned by CDI
            registrationSecurity2.CDIToken = new CdiServices.Vehicle.CDIToken();
            registrationSecurity2.CDIToken.Id = cdiTokenId;
            registrationSecurity2.CDIToken.TokenValue = cdiTokenValue;
            registrationSecurity2.CDIToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];

            // Set the CDISessionToken
            // This is an empty string until CDI returns a value for this in the response from this web method call
            registrationSecurity2.CDISessionToken = cdiSessionToken;

            // Create a client identifier object containing a MotoChek client id
            registrationSecurity2.ClientIdentifiers = new CdiServices.Vehicle.ClientIdentifiersType();
            registrationSecurity2.ClientIdentifiers.AccountId = new CdiServices.Vehicle.ClientIdentifiersTypeAccountId();
            registrationSecurity2.ClientIdentifiers.AccountId.AccountTypeSpecified = true;
            registrationSecurity2.ClientIdentifiers.AccountId.AccountType = CdiClient.CdiServices.Vehicle.ClientIdentifiersTypeAccountIdAccountType.MotoChek;
            registrationSecurity2.ClientIdentifiers.AccountId.Value = ConfigurationManager.AppSettings["ConfirmCurrentRegisteredPerson_AccountID"];

            // Set the soap header security
            vehicle.SecurityValue = registrationSecurity2;

            // Step 2.4 - Call the ConfirmCurrentRegisteredPerson web method
            CdiServices.Vehicle.InquireStolenVehicleResult confCurrRegPersonResponse = null;
            confCurrRegPersonResponse = vehicle.InquireStolenVehicle(inquireVehicleDetailsRequest);


            // Step 2.5 - Store the CDISessionToken returned from CDI
            // This is used by CDI to identify a client session
            if (vehicle.SecurityValue != null && vehicle.SecurityValue.CDISessionToken != null)
            {
                cdiSessionToken = vehicle.SecurityValue.CDISessionToken;
                Console.WriteLine("CDISessionToken '{0}' returned from CDI", cdiSessionToken);
            }

            // Step 2.6 - Check for messages returned from CDI
            if (confCurrRegPersonResponse.ServiceHeader != null && confCurrRegPersonResponse.ServiceHeader.Status != null && confCurrRegPersonResponse.ServiceHeader.Status.Messages != null &&
                confCurrRegPersonResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.Vehicle.MessageType messageType in confCurrRegPersonResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 2.7 - Check for an error code returned from CDI
            if (confCurrRegPersonResponse != null && confCurrRegPersonResponse.ServiceHeader != null && confCurrRegPersonResponse.ServiceHeader.Status != null &&
                confCurrRegPersonResponse.ServiceHeader.Status.Code != null && confCurrRegPersonResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI ConfirmCurrentRegisteredPerson web method completed ok");
            }
            else
            {
                cdiError = true;
                Console.WriteLine("CDI ConfirmCurrentRegisteredPerson web method did not complete ok");
            }


            #endregion

            #region Step 3.0 - Call the CDI EndSession web method

            // Step 3.1 - Create the AdministrationService web service proxy
            Console.WriteLine("Calling the CDI AdministrationService web service and EndSession web method ...");
            Console.WriteLine("- start -");
            CdiServices.AdministrationService.AdministrationService adminService = new CdiServices.AdministrationService.AdministrationService();


            // Step 3.2 - Create the request for the EndSession web method
            CdiServices.AdministrationService.EndSessionRequest endSessionRequest = new CdiServices.AdministrationService.EndSessionRequest();

            // Step 3.3 - Create the soap header security
            CdiServices.AdministrationService.Security adminServiceSecurity = new CdiServices.AdministrationService.Security();

            // Set the SecurityTokenReference.Id previously returned by CDI
            adminServiceSecurity.SecurityTokenReference = new CdiServices.AdministrationService.SecurityTokenReference();
            adminServiceSecurity.SecurityTokenReference.Id = cdiTokenId;

            // Set the CDIToken previously returned by CDI
            adminServiceSecurity.CDIToken = new CdiServices.AdministrationService.CDIToken();
            adminServiceSecurity.CDIToken.Id = cdiTokenId;
            adminServiceSecurity.CDIToken.TokenValue = cdiTokenValue;
            adminServiceSecurity.CDIToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];

            // Set the CDISessionToken previously returned by CDI
            adminServiceSecurity.CDISessionToken = cdiSessionToken;

            // Set the soap security header
            adminService.SecurityValue = adminServiceSecurity;

            // Step 3.4 - Call the EndSession web method
            CdiServices.AdministrationService.EndSessionResponse endSessionResponse = null;
            endSessionResponse = adminService.EndSession(endSessionRequest);

            // Step 3.5 - Check for messages returned from CDI
            if (endSessionResponse.ServiceHeader != null && endSessionResponse.ServiceHeader.Status != null && endSessionResponse.ServiceHeader.Status.Messages != null &&
                endSessionResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.AdministrationService.MessageType messageType in endSessionResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 3.6 - Check for an error code returned from CDI
            if (endSessionResponse != null && endSessionResponse.ServiceHeader != null && endSessionResponse.ServiceHeader.Status != null && endSessionResponse.ServiceHeader.Status.Code != null &&
                endSessionResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI EndSession web method completed ok");
            }
            else
            {
                Console.WriteLine("CDI EndSession web method did not complete ok");
            }
            // Step 2.8 - Retrieve the response data
            CdiServices.Vehicle.ArrayOfInquireStolenVehicleResponseResponseDetailResponseDetail[] responseDetail = confCurrRegPersonResponse.ResponseBody;
            CdiServices.Vehicle.ArrayOfInquireStolenVehicleResponseResponseDetailResponseDetail objReturn = new CdiServices.Vehicle.ArrayOfInquireStolenVehicleResponseResponseDetailResponseDetail();


            if (responseDetail.Count() > 0)
            {
                foreach (CdiServices.Vehicle.ArrayOfInquireStolenVehicleResponseResponseDetailResponseDetail reqType in responseDetail)
                {
                    if (reqType.ResponseTypeSpecified == true)
                    {
                        objReturn = reqType;
                    }
                }
            }

            return objReturn;


            #endregion

        }

        public static CdiServices.Vehicle.ArrayOfInquireVehicleUsageHistoryResponseResponseDetailResponseDetail getCurrentDetails(string rego)
        {
            Console.WriteLine();
            Console.WriteLine("Calling the CDI Confirm Current Registered Person transaction");
            Console.WriteLine();

            #region Local vars

            // These vars are used to retain values returned from CDI responses
            string cdiTokenId = null;               // The name of the CDIToken returned from CDI e.g. 'cdiToken'
            string cdiTokenValue = null;            // The value of the CDIToken returned from CDI e.g. '58be816a-8b84-4ab7-bdfd-20f2a946ff83'
            string cdiSessionToken = string.Empty;  // The value of the CDISessionToken returned from CDI e.g. '63ab802b-8b84-5ab7-bdgh-22f2a947ff82'
            bool cdiError = false;                  // Indicates that a CDI error was returned

            #endregion

            #region Step 1.0 - Call the CDI AuthenticateClient web method

            // Step 1.1 - Create AccessControl web service proxy
            Console.WriteLine("Calling the CDI AccessControl web service and AuthenticateClient web method");
            Console.WriteLine("- start -");
            CdiServices.AccessControl.AccessControl accessControlService = new CdiServices.AccessControl.AccessControl();

            // Step 1.2 - Create the request for the AuthenticateClient web method
            // The AuthenticateClient web method only requires an empty request to be input
            CdiServices.AccessControl.AuthenticateClientRequest authClientRequest = new CdiServices.AccessControl.AuthenticateClientRequest();
            authClientRequest.RequestBody = new object();

            // Step 1.3 - Create the soap header security

            // The UserNameToken is required by CDI to identify the client
            // CDI returns a CDIToken that uniquely identifies the client
            // The CDIToken must be submitted by the client with each subsequent request to CDI
            CdiServices.AccessControl.Security accessControlSecurity = new CdiServices.AccessControl.Security();
            accessControlSecurity.UserNameToken = new CdiServices.AccessControl.UserNameToken();
            accessControlSecurity.UserNameToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];     // The user name of the CDI client
            accessControlSecurity.UserNameToken.Password = ConfigurationManager.AppSettings["CdiPassword"];     // The password of the CDI client
            // Note: accessControlSecurity.UserNameToken.GroupName is optional

            // Set the soap security header
            accessControlService.SecurityValue = accessControlSecurity;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.SetTcpKeepAlive(false, 1000, 1000);

            CdiServices.AccessControl.AuthenticateClientResponse authClientResponse = null;
            authClientResponse = accessControlService.AuthenticateClient(authClientRequest);

            // Step 1.5 - Retain the CDIToken.Id and CDIToken.TokenValue returned from CDI
            // These need to be submitted with each subsequent web method call to CDI
            if (accessControlService.SecurityValue != null && accessControlService.SecurityValue.CDIToken != null && accessControlService.SecurityValue.CDIToken.Id != null)
            {
                cdiTokenId = accessControlService.SecurityValue.CDIToken.Id;
                Console.WriteLine("CDIToken.Id '{0}' returned from CDI", cdiTokenId);
            }

            if (accessControlService.SecurityValue != null && accessControlService.SecurityValue.CDIToken != null && accessControlService.SecurityValue.CDIToken.TokenValue != null)
            {
                cdiTokenValue = accessControlService.SecurityValue.CDIToken.TokenValue;
                Console.WriteLine("CDIToken.TokenValue {0} returned from CDI", cdiTokenValue);
            }

            // Step 1.6 - Retain the CDISessionToken returned from CDI
            // Thes needs to be submitted with each subsequent web method call to CDI
            if (accessControlSecurity != null && accessControlSecurity.CDISessionToken != null)
            {
                cdiSessionToken = accessControlService.SecurityValue.CDISessionToken;
                Console.WriteLine("CDISessionToken '{0}' returned from CDI", cdiSessionToken);
            }

            // Step 1.7 - Check for messages returned from CDI
            if (authClientResponse.ServiceHeader != null && authClientResponse.ServiceHeader.Status != null && authClientResponse.ServiceHeader.Status.Messages != null &&
                authClientResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.AccessControl.MessageType messageType in authClientResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 1.8 - Check for an error code returned from CDI
            if (authClientResponse != null && authClientResponse.ServiceHeader != null && authClientResponse.ServiceHeader.Status != null &&
                authClientResponse.ServiceHeader.Status.Code != null && authClientResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI AuthenticateClient web method completed ok");
            }
            else
            {
                cdiError = true;
                Console.WriteLine("CDI AuthenticateClient web method did not complete ok");
            }

            Console.WriteLine("- end -");
            Console.WriteLine();

            #endregion

            #region testVehicle
            CdiServices.Vehicle.Vehicle vehicle = new CdiServices.Vehicle.Vehicle();


            CdiServices.Vehicle.InquireVehicleUsageHistoryRequest inquireVehicleDetailsRequest = new CdiServices.Vehicle.InquireVehicleUsageHistoryRequest();
            inquireVehicleDetailsRequest.RequestBody = new CdiServices.Vehicle.VehicleOrPlateType[1];
            inquireVehicleDetailsRequest.RequestBody[0] = new CdiServices.Vehicle.VehicleOrPlateType();

            CdiServices.Vehicle.BasicPlateType basicPlate1 = new CdiClient.CdiServices.Vehicle.BasicPlateType();
            basicPlate1.PlateNumber = rego;
            inquireVehicleDetailsRequest.RequestBody[0].Item = basicPlate1;      // Add the plate to the request
                                                                                 // Step 2.3 - Create the soap header security
            CdiServices.Vehicle.Security registrationSecurity2 = new CdiServices.Vehicle.Security();

            // Set the SecurityTokenReference.Id previously returned by CDI
            // This holds the name/id of the CDI token e.g. 'cdiToken'
            registrationSecurity2.SecurityTokenReference = new CdiServices.Vehicle.SecurityTokenReference();
            registrationSecurity2.SecurityTokenReference.Id = cdiTokenId;

            // Set the CDIToken previously returned by CDI
            registrationSecurity2.CDIToken = new CdiServices.Vehicle.CDIToken();
            registrationSecurity2.CDIToken.Id = cdiTokenId;
            registrationSecurity2.CDIToken.TokenValue = cdiTokenValue;
            registrationSecurity2.CDIToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];

            // Set the CDISessionToken
            // This is an empty string until CDI returns a value for this in the response from this web method call
            registrationSecurity2.CDISessionToken = cdiSessionToken;

            // Create a client identifier object containing a MotoChek client id
            registrationSecurity2.ClientIdentifiers = new CdiServices.Vehicle.ClientIdentifiersType();
            registrationSecurity2.ClientIdentifiers.AccountId = new CdiServices.Vehicle.ClientIdentifiersTypeAccountId();
            registrationSecurity2.ClientIdentifiers.AccountId.AccountTypeSpecified = true;
            registrationSecurity2.ClientIdentifiers.AccountId.AccountType = CdiClient.CdiServices.Vehicle.ClientIdentifiersTypeAccountIdAccountType.MotoChek;
            registrationSecurity2.ClientIdentifiers.AccountId.Value = ConfigurationManager.AppSettings["ConfirmCurrentRegisteredPerson_AccountID"];

            // Set the soap header security
            vehicle.SecurityValue = registrationSecurity2;

            // Step 2.4 - Call the ConfirmCurrentRegisteredPerson web method
            CdiServices.Vehicle.InquireVehicleUsageHistoryResult confCurrRegPersonResponse = null;
            confCurrRegPersonResponse = vehicle.InquireVehicleUsageHistory(inquireVehicleDetailsRequest);


            // Step 2.5 - Store the CDISessionToken returned from CDI
            // This is used by CDI to identify a client session
            if (vehicle.SecurityValue != null && vehicle.SecurityValue.CDISessionToken != null)
            {
                cdiSessionToken = vehicle.SecurityValue.CDISessionToken;
                Console.WriteLine("CDISessionToken '{0}' returned from CDI", cdiSessionToken);
            }

            // Step 2.6 - Check for messages returned from CDI
            if (confCurrRegPersonResponse.ServiceHeader != null && confCurrRegPersonResponse.ServiceHeader.Status != null && confCurrRegPersonResponse.ServiceHeader.Status.Messages != null &&
                confCurrRegPersonResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.Vehicle.MessageType messageType in confCurrRegPersonResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 2.7 - Check for an error code returned from CDI
            if (confCurrRegPersonResponse != null && confCurrRegPersonResponse.ServiceHeader != null && confCurrRegPersonResponse.ServiceHeader.Status != null &&
                confCurrRegPersonResponse.ServiceHeader.Status.Code != null && confCurrRegPersonResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI ConfirmCurrentRegisteredPerson web method completed ok");
            }
            else
            {
                cdiError = true;
                Console.WriteLine("CDI ConfirmCurrentRegisteredPerson web method did not complete ok");
            }


            #endregion

            #region Step 3.0 - Call the CDI EndSession web method

            // Step 3.1 - Create the AdministrationService web service proxy
            Console.WriteLine("Calling the CDI AdministrationService web service and EndSession web method ...");
            Console.WriteLine("- start -");
            CdiServices.AdministrationService.AdministrationService adminService = new CdiServices.AdministrationService.AdministrationService();


            // Step 3.2 - Create the request for the EndSession web method
            CdiServices.AdministrationService.EndSessionRequest endSessionRequest = new CdiServices.AdministrationService.EndSessionRequest();

            // Step 3.3 - Create the soap header security
            CdiServices.AdministrationService.Security adminServiceSecurity = new CdiServices.AdministrationService.Security();

            // Set the SecurityTokenReference.Id previously returned by CDI
            adminServiceSecurity.SecurityTokenReference = new CdiServices.AdministrationService.SecurityTokenReference();
            adminServiceSecurity.SecurityTokenReference.Id = cdiTokenId;

            // Set the CDIToken previously returned by CDI
            adminServiceSecurity.CDIToken = new CdiServices.AdministrationService.CDIToken();
            adminServiceSecurity.CDIToken.Id = cdiTokenId;
            adminServiceSecurity.CDIToken.TokenValue = cdiTokenValue;
            adminServiceSecurity.CDIToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];

            // Set the CDISessionToken previously returned by CDI
            adminServiceSecurity.CDISessionToken = cdiSessionToken;

            // Set the soap security header
            adminService.SecurityValue = adminServiceSecurity;

            // Step 3.4 - Call the EndSession web method
            CdiServices.AdministrationService.EndSessionResponse endSessionResponse = null;
            endSessionResponse = adminService.EndSession(endSessionRequest);

            // Step 3.5 - Check for messages returned from CDI
            if (endSessionResponse.ServiceHeader != null && endSessionResponse.ServiceHeader.Status != null && endSessionResponse.ServiceHeader.Status.Messages != null &&
                endSessionResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.AdministrationService.MessageType messageType in endSessionResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 3.6 - Check for an error code returned from CDI
            if (endSessionResponse != null && endSessionResponse.ServiceHeader != null && endSessionResponse.ServiceHeader.Status != null && endSessionResponse.ServiceHeader.Status.Code != null &&
                endSessionResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI EndSession web method completed ok");
            }
            else
            {
                Console.WriteLine("CDI EndSession web method did not complete ok");
            }
            // Step 2.8 - Retrieve the response data
            CdiServices.Vehicle.ArrayOfInquireVehicleUsageHistoryResponseResponseDetailResponseDetail[] responseDetail = confCurrRegPersonResponse.ResponseBody;
            CdiServices.Vehicle.ArrayOfInquireVehicleUsageHistoryResponseResponseDetailResponseDetail objReturn = new CdiServices.Vehicle.ArrayOfInquireVehicleUsageHistoryResponseResponseDetailResponseDetail();


            if (responseDetail.Count() > 0)
            {
                foreach (CdiServices.Vehicle.ArrayOfInquireVehicleUsageHistoryResponseResponseDetailResponseDetail reqType in responseDetail)
                {
                    if (reqType.ResponseTypeSpecified == true)
                    {
                        objReturn = reqType;
                    }
                }
            }

            return objReturn;


            #endregion

        }

        public static CdiServices.Vehicle.ArrayOfInquireVehicleDetailsResponseResponseDetailResponseDetail getVehicleDetails(string rego)
        {
            Console.WriteLine();
            Console.WriteLine("Calling the CDI Confirm Current Registered Person transaction");
            Console.WriteLine();

            #region Local vars

            // These vars are used to retain values returned from CDI responses
            string cdiTokenId = null;               // The name of the CDIToken returned from CDI e.g. 'cdiToken'
            string cdiTokenValue = null;            // The value of the CDIToken returned from CDI e.g. '58be816a-8b84-4ab7-bdfd-20f2a946ff83'
            string cdiSessionToken = string.Empty;  // The value of the CDISessionToken returned from CDI e.g. '63ab802b-8b84-5ab7-bdgh-22f2a947ff82'
            bool cdiError = false;                  // Indicates that a CDI error was returned

            #endregion

            #region Step 1.0 - Call the CDI AuthenticateClient web method

            // Step 1.1 - Create AccessControl web service proxy
            Console.WriteLine("Calling the CDI AccessControl web service and AuthenticateClient web method");
            Console.WriteLine("- start -");
            CdiServices.AccessControl.AccessControl accessControlService = new CdiServices.AccessControl.AccessControl();

            // Step 1.2 - Create the request for the AuthenticateClient web method
            // The AuthenticateClient web method only requires an empty request to be input
            CdiServices.AccessControl.AuthenticateClientRequest authClientRequest = new CdiServices.AccessControl.AuthenticateClientRequest();
            authClientRequest.RequestBody = new object();

            // Step 1.3 - Create the soap header security

            // The UserNameToken is required by CDI to identify the client
            // CDI returns a CDIToken that uniquely identifies the client
            // The CDIToken must be submitted by the client with each subsequent request to CDI
            CdiServices.AccessControl.Security accessControlSecurity = new CdiServices.AccessControl.Security();
            accessControlSecurity.UserNameToken = new CdiServices.AccessControl.UserNameToken();
            accessControlSecurity.UserNameToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];     // The user name of the CDI client
            accessControlSecurity.UserNameToken.Password = ConfigurationManager.AppSettings["CdiPassword"];     // The password of the CDI client
            // Note: accessControlSecurity.UserNameToken.GroupName is optional

            // Set the soap security header
            accessControlService.SecurityValue = accessControlSecurity;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.SetTcpKeepAlive(false, 1000, 1000);
            // Step 1.4 - Call the AuthenticateClient web method
            CdiServices.AccessControl.AuthenticateClientResponse authClientResponse = null;
            authClientResponse = accessControlService.AuthenticateClient(authClientRequest);

            // Step 1.5 - Retain the CDIToken.Id and CDIToken.TokenValue returned from CDI
            // These need to be submitted with each subsequent web method call to CDI
            if (accessControlService.SecurityValue != null && accessControlService.SecurityValue.CDIToken != null && accessControlService.SecurityValue.CDIToken.Id != null)
            {
                cdiTokenId = accessControlService.SecurityValue.CDIToken.Id;
                Console.WriteLine("CDIToken.Id '{0}' returned from CDI", cdiTokenId);
            }

            if (accessControlService.SecurityValue != null && accessControlService.SecurityValue.CDIToken != null && accessControlService.SecurityValue.CDIToken.TokenValue != null)
            {
                cdiTokenValue = accessControlService.SecurityValue.CDIToken.TokenValue;
                Console.WriteLine("CDIToken.TokenValue {0} returned from CDI", cdiTokenValue);
            }

            // Step 1.6 - Retain the CDISessionToken returned from CDI
            // Thes needs to be submitted with each subsequent web method call to CDI
            if (accessControlSecurity != null && accessControlSecurity.CDISessionToken != null)
            {
                cdiSessionToken = accessControlService.SecurityValue.CDISessionToken;
                Console.WriteLine("CDISessionToken '{0}' returned from CDI", cdiSessionToken);
            }

            // Step 1.7 - Check for messages returned from CDI
            if (authClientResponse.ServiceHeader != null && authClientResponse.ServiceHeader.Status != null && authClientResponse.ServiceHeader.Status.Messages != null &&
                authClientResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.AccessControl.MessageType messageType in authClientResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 1.8 - Check for an error code returned from CDI
            if (authClientResponse != null && authClientResponse.ServiceHeader != null && authClientResponse.ServiceHeader.Status != null &&
                authClientResponse.ServiceHeader.Status.Code != null && authClientResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI AuthenticateClient web method completed ok");
            }
            else
            {
                cdiError = true;
                Console.WriteLine("CDI AuthenticateClient web method did not complete ok");
            }

            Console.WriteLine("- end -");
            Console.WriteLine();

            #endregion

            #region testVehicle
            CdiServices.Vehicle.Vehicle vehicle = new CdiServices.Vehicle.Vehicle();


            CdiServices.Vehicle.InquireVehicleDetailsRequest inquireVehicleDetailsRequest = new CdiServices.Vehicle.InquireVehicleDetailsRequest();
            inquireVehicleDetailsRequest.RequestBody = new CdiServices.Vehicle.VehicleOrPlateType[1];
            inquireVehicleDetailsRequest.RequestBody[0] = new CdiServices.Vehicle.VehicleOrPlateType();

            CdiServices.Vehicle.BasicPlateType basicPlate1 = new CdiClient.CdiServices.Vehicle.BasicPlateType();
            basicPlate1.PlateNumber = rego;
            inquireVehicleDetailsRequest.RequestBody[0].Item = basicPlate1;      // Add the plate to the request
                                                                                 // Step 2.3 - Create the soap header security
            CdiServices.Vehicle.Security registrationSecurity2 = new CdiServices.Vehicle.Security();

            // Set the SecurityTokenReference.Id previously returned by CDI
            // This holds the name/id of the CDI token e.g. 'cdiToken'
            registrationSecurity2.SecurityTokenReference = new CdiServices.Vehicle.SecurityTokenReference();
            registrationSecurity2.SecurityTokenReference.Id = cdiTokenId;

            // Set the CDIToken previously returned by CDI
            registrationSecurity2.CDIToken = new CdiServices.Vehicle.CDIToken();
            registrationSecurity2.CDIToken.Id = cdiTokenId;
            registrationSecurity2.CDIToken.TokenValue = cdiTokenValue;
            registrationSecurity2.CDIToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];

            // Set the CDISessionToken
            // This is an empty string until CDI returns a value for this in the response from this web method call
            registrationSecurity2.CDISessionToken = cdiSessionToken;

            // Create a client identifier object containing a MotoChek client id
            registrationSecurity2.ClientIdentifiers = new CdiServices.Vehicle.ClientIdentifiersType();
            registrationSecurity2.ClientIdentifiers.AccountId = new CdiServices.Vehicle.ClientIdentifiersTypeAccountId();
            registrationSecurity2.ClientIdentifiers.AccountId.AccountTypeSpecified = true;
            registrationSecurity2.ClientIdentifiers.AccountId.AccountType = CdiClient.CdiServices.Vehicle.ClientIdentifiersTypeAccountIdAccountType.MotoChek;
            registrationSecurity2.ClientIdentifiers.AccountId.Value = ConfigurationManager.AppSettings["ConfirmCurrentRegisteredPerson_AccountID"];

            // Set the soap header security
            vehicle.SecurityValue = registrationSecurity2;

            // Step 2.4 - Call the ConfirmCurrentRegisteredPerson web method
            CdiServices.Vehicle.InquireVehicleDetailsResult confCurrRegPersonResponse = null;
            confCurrRegPersonResponse = vehicle.InquireVehicleDetails(inquireVehicleDetailsRequest);


            // Step 2.5 - Store the CDISessionToken returned from CDI
            // This is used by CDI to identify a client session
            if (vehicle.SecurityValue != null && vehicle.SecurityValue.CDISessionToken != null)
            {
                cdiSessionToken = vehicle.SecurityValue.CDISessionToken;
                Console.WriteLine("CDISessionToken '{0}' returned from CDI", cdiSessionToken);
            }

            // Step 2.6 - Check for messages returned from CDI
            if (confCurrRegPersonResponse.ServiceHeader != null && confCurrRegPersonResponse.ServiceHeader.Status != null && confCurrRegPersonResponse.ServiceHeader.Status.Messages != null &&
                confCurrRegPersonResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.Vehicle.MessageType messageType in confCurrRegPersonResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 2.7 - Check for an error code returned from CDI
            if (confCurrRegPersonResponse != null && confCurrRegPersonResponse.ServiceHeader != null && confCurrRegPersonResponse.ServiceHeader.Status != null &&
                confCurrRegPersonResponse.ServiceHeader.Status.Code != null && confCurrRegPersonResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI ConfirmCurrentRegisteredPerson web method completed ok");
            }
            else
            {
                cdiError = true;
                Console.WriteLine("CDI ConfirmCurrentRegisteredPerson web method did not complete ok");
            }


            #endregion


            #region Step 3.0 - Call the CDI EndSession web method

            // Step 3.1 - Create the AdministrationService web service proxy
            Console.WriteLine("Calling the CDI AdministrationService web service and EndSession web method ...");
            Console.WriteLine("- start -");
            CdiServices.AdministrationService.AdministrationService adminService = new CdiServices.AdministrationService.AdministrationService();


            // Step 3.2 - Create the request for the EndSession web method
            CdiServices.AdministrationService.EndSessionRequest endSessionRequest = new CdiServices.AdministrationService.EndSessionRequest();

            // Step 3.3 - Create the soap header security
            CdiServices.AdministrationService.Security adminServiceSecurity = new CdiServices.AdministrationService.Security();

            // Set the SecurityTokenReference.Id previously returned by CDI
            adminServiceSecurity.SecurityTokenReference = new CdiServices.AdministrationService.SecurityTokenReference();
            adminServiceSecurity.SecurityTokenReference.Id = cdiTokenId;

            // Set the CDIToken previously returned by CDI
            adminServiceSecurity.CDIToken = new CdiServices.AdministrationService.CDIToken();
            adminServiceSecurity.CDIToken.Id = cdiTokenId;
            adminServiceSecurity.CDIToken.TokenValue = cdiTokenValue;
            adminServiceSecurity.CDIToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];

            // Set the CDISessionToken previously returned by CDI
            adminServiceSecurity.CDISessionToken = cdiSessionToken;

            // Set the soap security header
            adminService.SecurityValue = adminServiceSecurity;

            // Step 3.4 - Call the EndSession web method
            CdiServices.AdministrationService.EndSessionResponse endSessionResponse = null;
            endSessionResponse = adminService.EndSession(endSessionRequest);

            // Step 3.5 - Check for messages returned from CDI
            if (endSessionResponse.ServiceHeader != null && endSessionResponse.ServiceHeader.Status != null && endSessionResponse.ServiceHeader.Status.Messages != null &&
                endSessionResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.AdministrationService.MessageType messageType in endSessionResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 3.6 - Check for an error code returned from CDI
            if (endSessionResponse != null && endSessionResponse.ServiceHeader != null && endSessionResponse.ServiceHeader.Status != null && endSessionResponse.ServiceHeader.Status.Code != null &&
                endSessionResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI EndSession web method completed ok");
            }
            else
            {
                Console.WriteLine("CDI EndSession web method did not complete ok");
            }

            // Step 2.8 - Retrieve the response data
            CdiServices.Vehicle.ArrayOfInquireVehicleDetailsResponseResponseDetailResponseDetail[] responseDetail = confCurrRegPersonResponse.ResponseBody;
            CdiServices.Vehicle.ArrayOfInquireVehicleDetailsResponseResponseDetailResponseDetail objReturn = new CdiServices.Vehicle.ArrayOfInquireVehicleDetailsResponseResponseDetailResponseDetail();
            if (responseDetail.Count() > 0)
            {
                foreach (CdiServices.Vehicle.ArrayOfInquireVehicleDetailsResponseResponseDetailResponseDetail reqType in responseDetail)
                {
                    if (reqType.ResponseTypeSpecified == true)
                    {
                        objReturn = reqType;
                    }
                }
            }

            return objReturn;

            #endregion

        }

        public static CdiServices.RucLicence.ArrayOfInquireLatestRucLicenceDetailsResponseResponseDetailResponseDetail getRUC(string rego)
        {
            Console.WriteLine();
            Console.WriteLine("Calling the CDI Confirm Current Registered Person transaction");
            Console.WriteLine();

            #region Local vars

            // These vars are used to retain values returned from CDI responses
            string cdiTokenId = null;               // The name of the CDIToken returned from CDI e.g. 'cdiToken'
            string cdiTokenValue = null;            // The value of the CDIToken returned from CDI e.g. '58be816a-8b84-4ab7-bdfd-20f2a946ff83'
            string cdiSessionToken = string.Empty;  // The value of the CDISessionToken returned from CDI e.g. '63ab802b-8b84-5ab7-bdgh-22f2a947ff82'
            bool cdiError = false;                  // Indicates that a CDI error was returned

            #endregion

            #region Step 1.0 - Call the CDI AuthenticateClient web method

            // Step 1.1 - Create AccessControl web service proxy
            Console.WriteLine("Calling the CDI AccessControl web service and AuthenticateClient web method");
            Console.WriteLine("- start -");
            CdiServices.AccessControl.AccessControl accessControlService = new CdiServices.AccessControl.AccessControl();

            // Step 1.2 - Create the request for the AuthenticateClient web method
            // The AuthenticateClient web method only requires an empty request to be input
            CdiServices.AccessControl.AuthenticateClientRequest authClientRequest = new CdiServices.AccessControl.AuthenticateClientRequest();
            authClientRequest.RequestBody = new object();

            // Step 1.3 - Create the soap header security

            // The UserNameToken is required by CDI to identify the client
            // CDI returns a CDIToken that uniquely identifies the client
            // The CDIToken must be submitted by the client with each subsequent request to CDI
            CdiServices.AccessControl.Security accessControlSecurity = new CdiServices.AccessControl.Security();
            accessControlSecurity.UserNameToken = new CdiServices.AccessControl.UserNameToken();
            accessControlSecurity.UserNameToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];     // The user name of the CDI client
            accessControlSecurity.UserNameToken.Password = ConfigurationManager.AppSettings["CdiPassword"];     // The password of the CDI client
            // Note: accessControlSecurity.UserNameToken.GroupName is optional

            // Set the soap security header
            accessControlService.SecurityValue = accessControlSecurity;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.SetTcpKeepAlive(false, 1000, 1000);
            
            // Step 1.4 - Call the AuthenticateClient web method
            CdiServices.AccessControl.AuthenticateClientResponse authClientResponse = null;
            authClientResponse = accessControlService.AuthenticateClient(authClientRequest);

            // Step 1.5 - Retain the CDIToken.Id and CDIToken.TokenValue returned from CDI
            // These need to be submitted with each subsequent web method call to CDI
            if (accessControlService.SecurityValue != null && accessControlService.SecurityValue.CDIToken != null && accessControlService.SecurityValue.CDIToken.Id != null)
            {
                cdiTokenId = accessControlService.SecurityValue.CDIToken.Id;
                Console.WriteLine("CDIToken.Id '{0}' returned from CDI", cdiTokenId);
            }

            if (accessControlService.SecurityValue != null && accessControlService.SecurityValue.CDIToken != null && accessControlService.SecurityValue.CDIToken.TokenValue != null)
            {
                cdiTokenValue = accessControlService.SecurityValue.CDIToken.TokenValue;
                Console.WriteLine("CDIToken.TokenValue {0} returned from CDI", cdiTokenValue);
            }

            // Step 1.6 - Retain the CDISessionToken returned from CDI
            // Thes needs to be submitted with each subsequent web method call to CDI
            if (accessControlSecurity != null && accessControlSecurity.CDISessionToken != null)
            {
                cdiSessionToken = accessControlService.SecurityValue.CDISessionToken;
                Console.WriteLine("CDISessionToken '{0}' returned from CDI", cdiSessionToken);
            }

            // Step 1.7 - Check for messages returned from CDI
            if (authClientResponse.ServiceHeader != null && authClientResponse.ServiceHeader.Status != null && authClientResponse.ServiceHeader.Status.Messages != null &&
                authClientResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.AccessControl.MessageType messageType in authClientResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 1.8 - Check for an error code returned from CDI
            if (authClientResponse != null && authClientResponse.ServiceHeader != null && authClientResponse.ServiceHeader.Status != null &&
                authClientResponse.ServiceHeader.Status.Code != null && authClientResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI AuthenticateClient web method completed ok");
            }
            else
            {
                cdiError = true;
                Console.WriteLine("CDI AuthenticateClient web method did not complete ok");
            }

            Console.WriteLine("- end -");
            Console.WriteLine();

            #endregion

            #region testVehicle


            CdiServices.RucLicence.RucLicence rucLicence = new CdiServices.RucLicence.RucLicence();


            CdiServices.RucLicence.InquireLatestRucLicenceDetailsRequest inquireLatestRucLicenceDetailsRequest = new CdiServices.RucLicence.InquireLatestRucLicenceDetailsRequest();
            inquireLatestRucLicenceDetailsRequest.RequestBody = new CdiServices.RucLicence.VehicleOrPlateType[1];
            inquireLatestRucLicenceDetailsRequest.RequestBody[0] = new CdiServices.RucLicence.VehicleOrPlateType();


            CdiServices.RucLicence.BasicPlateType basicPlate1 = new CdiClient.CdiServices.RucLicence.BasicPlateType();
            basicPlate1.PlateNumber = rego;
            inquireLatestRucLicenceDetailsRequest.RequestBody[0].Item = basicPlate1;


            CdiServices.RucLicence.Security registrationSecurity2 = new CdiServices.RucLicence.Security();

            // Set the SecurityTokenReference.Id previously returned by CDI
            // This holds the name/id of the CDI token e.g. 'cdiToken'
            registrationSecurity2.SecurityTokenReference = new CdiServices.RucLicence.SecurityTokenReference();
            registrationSecurity2.SecurityTokenReference.Id = cdiTokenId;

            // Set the CDIToken previously returned by CDI
            registrationSecurity2.CDIToken = new CdiServices.RucLicence.CDIToken();
            registrationSecurity2.CDIToken.Id = cdiTokenId;
            registrationSecurity2.CDIToken.TokenValue = cdiTokenValue;
            registrationSecurity2.CDIToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];

            // Set the CDISessionToken
            // This is an empty string until CDI returns a value for this in the response from this web method call
            registrationSecurity2.CDISessionToken = cdiSessionToken;

            // Create a client identifier object containing a MotoChek client id
            registrationSecurity2.ClientIdentifiers = new CdiServices.RucLicence.ClientIdentifiersType();
            registrationSecurity2.ClientIdentifiers.AccountId = new CdiServices.RucLicence.ClientIdentifiersTypeAccountId();
            registrationSecurity2.ClientIdentifiers.AccountId.AccountTypeSpecified = true;
            registrationSecurity2.ClientIdentifiers.AccountId.AccountType = CdiClient.CdiServices.RucLicence.ClientIdentifiersTypeAccountIdAccountType.MotoChek;
            registrationSecurity2.ClientIdentifiers.AccountId.Value = ConfigurationManager.AppSettings["ConfirmCurrentRegisteredPerson_AccountID"];

            // Set the soap header security
            rucLicence.SecurityValue = registrationSecurity2;



            // Step 2.4 - Call the ConfirmCurrentRegisteredPerson web method
            CdiServices.RucLicence.InquireLatestRucLicenceDetailsResult confCurrRegPersonResponse = null;
            confCurrRegPersonResponse = rucLicence.InquireLatestRucLicenceDetails(inquireLatestRucLicenceDetailsRequest);


            // Step 2.5 - Store the CDISessionToken returned from CDI
            // This is used by CDI to identify a client session
            if (rucLicence.SecurityValue != null && rucLicence.SecurityValue.CDISessionToken != null)
            {
                cdiSessionToken = rucLicence.SecurityValue.CDISessionToken;
                Console.WriteLine("CDISessionToken '{0}' returned from CDI", cdiSessionToken);
            }

            // Step 2.6 - Check for messages returned from CDI
            if (confCurrRegPersonResponse.ServiceHeader != null && confCurrRegPersonResponse.ServiceHeader.Status != null && confCurrRegPersonResponse.ServiceHeader.Status.Messages != null &&
                confCurrRegPersonResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.RucLicence.MessageType messageType in confCurrRegPersonResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 2.7 - Check for an error code returned from CDI
            if (confCurrRegPersonResponse != null && confCurrRegPersonResponse.ServiceHeader != null && confCurrRegPersonResponse.ServiceHeader.Status != null &&
                confCurrRegPersonResponse.ServiceHeader.Status.Code != null && confCurrRegPersonResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI ConfirmCurrentRegisteredPerson web method completed ok");
            }
            else
            {
                cdiError = true;
                Console.WriteLine("CDI ConfirmCurrentRegisteredPerson web method did not complete ok");
            }


            #endregion


            #region Step 3.0 - Call the CDI EndSession web method

            // Step 3.1 - Create the AdministrationService web service proxy
            Console.WriteLine("Calling the CDI AdministrationService web service and EndSession web method ...");
            Console.WriteLine("- start -");
            CdiServices.AdministrationService.AdministrationService adminService = new CdiServices.AdministrationService.AdministrationService();


            // Step 3.2 - Create the request for the EndSession web method
            CdiServices.AdministrationService.EndSessionRequest endSessionRequest = new CdiServices.AdministrationService.EndSessionRequest();

            // Step 3.3 - Create the soap header security
            CdiServices.AdministrationService.Security adminServiceSecurity = new CdiServices.AdministrationService.Security();

            // Set the SecurityTokenReference.Id previously returned by CDI
            adminServiceSecurity.SecurityTokenReference = new CdiServices.AdministrationService.SecurityTokenReference();
            adminServiceSecurity.SecurityTokenReference.Id = cdiTokenId;

            // Set the CDIToken previously returned by CDI
            adminServiceSecurity.CDIToken = new CdiServices.AdministrationService.CDIToken();
            adminServiceSecurity.CDIToken.Id = cdiTokenId;
            adminServiceSecurity.CDIToken.TokenValue = cdiTokenValue;
            adminServiceSecurity.CDIToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];

            // Set the CDISessionToken previously returned by CDI
            adminServiceSecurity.CDISessionToken = cdiSessionToken;

            // Set the soap security header
            adminService.SecurityValue = adminServiceSecurity;

            // Step 3.4 - Call the EndSession web method
            CdiServices.AdministrationService.EndSessionResponse endSessionResponse = null;
            endSessionResponse = adminService.EndSession(endSessionRequest);

            // Step 3.5 - Check for messages returned from CDI
            if (endSessionResponse.ServiceHeader != null && endSessionResponse.ServiceHeader.Status != null && endSessionResponse.ServiceHeader.Status.Messages != null &&
                endSessionResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.AdministrationService.MessageType messageType in endSessionResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 3.6 - Check for an error code returned from CDI
            if (endSessionResponse != null && endSessionResponse.ServiceHeader != null && endSessionResponse.ServiceHeader.Status != null && endSessionResponse.ServiceHeader.Status.Code != null &&
                endSessionResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI EndSession web method completed ok");
            }
            else
            {
                Console.WriteLine("CDI EndSession web method did not complete ok");
            }

            // Step 2.8 - Retrieve the response data
            CdiServices.RucLicence.ArrayOfInquireLatestRucLicenceDetailsResponseResponseDetailResponseDetail[] responseDetail = confCurrRegPersonResponse.ResponseBody;
            CdiServices.RucLicence.ArrayOfInquireLatestRucLicenceDetailsResponseResponseDetailResponseDetail objReturn = new CdiServices.RucLicence.ArrayOfInquireLatestRucLicenceDetailsResponseResponseDetailResponseDetail();


            if (responseDetail.Count() > 0)
            {
                foreach (CdiServices.RucLicence.ArrayOfInquireLatestRucLicenceDetailsResponseResponseDetailResponseDetail reqType in responseDetail)
                {
                    if (reqType.ResponseTypeSpecified == true)
                    {
                        objReturn = reqType;
                    }
                }
            }

            return objReturn;

            #endregion

        }

        public static CdiServices.Registration.ArrayOfConfirmCurrentRegisteredPersonResponseResponseDetailResponseDetail getCurrentOwner(string rego, string companyName, string licence, string firstName, string lastName)
        {
            Console.WriteLine();
            Console.WriteLine("Calling the CDI Confirm Current Registered Person transaction");
            Console.WriteLine();
            CdiServices.Registration.ArrayOfConfirmCurrentRegisteredPersonResponseResponseDetailResponseDetail objReturn = new CdiServices.Registration.ArrayOfConfirmCurrentRegisteredPersonResponseResponseDetailResponseDetail();
            #region Local vars

            // These vars are used to retain values returned from CDI responses
            string cdiTokenId = null;               // The name of the CDIToken returned from CDI e.g. 'cdiToken'
            string cdiTokenValue = null;            // The value of the CDIToken returned from CDI e.g. '58be816a-8b84-4ab7-bdfd-20f2a946ff83'
            string cdiSessionToken = string.Empty;  // The value of the CDISessionToken returned from CDI e.g. '63ab802b-8b84-5ab7-bdgh-22f2a947ff82'
            bool cdiError = false;                  // Indicates that a CDI error was returned

            #endregion

            #region Step 1.0 - Call the CDI AuthenticateClient web method

            // Step 1.1 - Create AccessControl web service proxy
            Console.WriteLine("Calling the CDI AccessControl web service and AuthenticateClient web method");
            Console.WriteLine("- start -");
            CdiServices.AccessControl.AccessControl accessControlService = new CdiServices.AccessControl.AccessControl();

            // Step 1.2 - Create the request for the AuthenticateClient web method
            // The AuthenticateClient web method only requires an empty request to be input
            CdiServices.AccessControl.AuthenticateClientRequest authClientRequest = new CdiServices.AccessControl.AuthenticateClientRequest();
            authClientRequest.RequestBody = new object();

            // Step 1.3 - Create the soap header security

            // The UserNameToken is required by CDI to identify the client
            // CDI returns a CDIToken that uniquely identifies the client
            // The CDIToken must be submitted by the client with each subsequent request to CDI
            CdiServices.AccessControl.Security accessControlSecurity = new CdiServices.AccessControl.Security();
            accessControlSecurity.UserNameToken = new CdiServices.AccessControl.UserNameToken();
            accessControlSecurity.UserNameToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];     // The user name of the CDI client
            accessControlSecurity.UserNameToken.Password = ConfigurationManager.AppSettings["CdiPassword"];     // The password of the CDI client
                                                                                                                // Note: accessControlSecurity.UserNameToken.GroupName is optional

            // Set the soap security header
            accessControlService.SecurityValue = accessControlSecurity;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.SetTcpKeepAlive(false, 1000, 1000);

            // Step 1.4 - Call the AuthenticateClient web method
            CdiServices.AccessControl.AuthenticateClientResponse authClientResponse = null;
            authClientResponse = accessControlService.AuthenticateClient(authClientRequest);

            // Step 1.5 - Retain the CDIToken.Id and CDIToken.TokenValue returned from CDI
            // These need to be submitted with each subsequent web method call to CDI
            if (accessControlService.SecurityValue != null && accessControlService.SecurityValue.CDIToken != null && accessControlService.SecurityValue.CDIToken.Id != null)
            {
                cdiTokenId = accessControlService.SecurityValue.CDIToken.Id;
                Console.WriteLine("CDIToken.Id '{0}' returned from CDI", cdiTokenId);
            }
            if (accessControlService.SecurityValue != null && accessControlService.SecurityValue.CDIToken != null && accessControlService.SecurityValue.CDIToken.TokenValue != null)
            {
                cdiTokenValue = accessControlService.SecurityValue.CDIToken.TokenValue;
                Console.WriteLine("CDIToken.TokenValue {0} returned from CDI", cdiTokenValue);
            }

            // Step 1.6 - Retain the CDISessionToken returned from CDI
            // Thes needs to be submitted with each subsequent web method call to CDI
            if (accessControlSecurity != null && accessControlSecurity.CDISessionToken != null)
            {
                cdiSessionToken = accessControlService.SecurityValue.CDISessionToken;
                Console.WriteLine("CDISessionToken '{0}' returned from CDI", cdiSessionToken);
            }

            // Step 1.7 - Check for messages returned from CDI
            if (authClientResponse.ServiceHeader != null && authClientResponse.ServiceHeader.Status != null && authClientResponse.ServiceHeader.Status.Messages != null &&
                authClientResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.AccessControl.MessageType messageType in authClientResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 1.8 - Check for an error code returned from CDI
            if (authClientResponse != null && authClientResponse.ServiceHeader != null && authClientResponse.ServiceHeader.Status != null &&
                authClientResponse.ServiceHeader.Status.Code != null && authClientResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI AuthenticateClient web method completed ok");
            }
            else
            {
                cdiError = true;
                Console.WriteLine("CDI AuthenticateClient web method did not complete ok");
            }

            Console.WriteLine("- end -");
            Console.WriteLine();

            #endregion

            #region Step 2.0 - Call the CDI ConfirmCurrentRegisteredPerson web method

            if (cdiError == false)
            {
                // Step 2.1 - Create the ConfirmCurrentRegisteredPerson web service proxy
                Console.WriteLine("Calling the CDI Registration web service and ConfirmCurrentRegisteredPerson web method ...");
                Console.WriteLine("- start -");
                CdiServices.Registration.Registration registrationService = new CdiServices.Registration.Registration();

                // Step 2.2 - Create the request for the ConfirmCurrentRegisteredPerson web method
                CdiServices.Registration.ConfirmCurrentRegisteredPersonRequest confCurrRegPersonRequest = new CdiClient.CdiServices.Registration.ConfirmCurrentRegisteredPersonRequest();
                confCurrRegPersonRequest.RequestBody = new CdiServices.Registration.ConfirmCurrentRegisteredPersonRequestType[1];
                confCurrRegPersonRequest.RequestBody[0] = new CdiServices.Registration.ConfirmCurrentRegisteredPersonRequestType();
                confCurrRegPersonRequest.RequestBody[0].Item = new CdiServices.Registration.VehicleOrPlateType();

                // Create a party object
                CdiServices.Registration.PartyType party = new CdiServices.Registration.PartyType();
                party.PartyType1Specified = true;
                party.PartyType1 = CdiClient.CdiServices.Registration.PartyTypeList.Individual;

                // Create a VehicleOrPlateRequestType that can hold either a BasicPlateType or a BasicVehicleDetailsType
                CdiServices.Registration.BasicPlateType basicPlate = new CdiClient.CdiServices.Registration.BasicPlateType();
                basicPlate.PlateNumber = rego;
                confCurrRegPersonRequest.RequestBody[0].Item = basicPlate;      // Add the plate to the request

                if (licence == "" && companyName == "")
                {
                // Create a person name object containing a person's name
                party.PartyName = new CdiServices.Registration.PartyNameType();
                party.PartyName.PersonName = new CdiServices.Registration.PartyNameTypePersonName[1];
                party.PartyName.PersonName[0] = new CdiClient.CdiServices.Registration.PartyNameTypePersonName();
                party.PartyName.PersonName[0].NameElement = new CdiServices.Registration.PersonNameTypeNameElement[2];
                party.PartyName.PersonName[0].NameElement[0] = new CdiServices.Registration.PersonNameTypeNameElement();
                party.PartyName.PersonName[0].NameElement[0].ElementTypeSpecified = true;
                party.PartyName.PersonName[0].NameElement[0].ElementType = CdiServices.Registration.PersonNameElementList.FirstName;
                party.PartyName.PersonName[0].NameElement[0].Value = firstName;
                party.PartyName.PersonName[0].NameElement[1] = new CdiServices.Registration.PersonNameTypeNameElement();
                party.PartyName.PersonName[0].NameElement[1].ElementTypeSpecified = true;
                party.PartyName.PersonName[0].NameElement[1].ElementType = CdiServices.Registration.PersonNameElementList.LastName;
                party.PartyName.PersonName[0].NameElement[1].Value = lastName;

                }

                // Create a birth info object containing a person's birth date
                //party.BirthInfo = new CdiServices.Registration.PartyTypeBirthInfo();
                // party.BirthInfo.BirthDateTime = Convert.ToDateTime(date);

                if(companyName == "" && licence != "")
                {

                    // Create a document object containing a person's driver license
                party.PartyName = new CdiServices.Registration.PartyNameType();
                party.PartyName.OrganisationName = new CdiServices.Registration.PartyNameTypeOrganisationName[1];
                party.PartyName.OrganisationName[0] = new CdiClient.CdiServices.Registration.PartyNameTypeOrganisationName();
                party.PartyName.OrganisationName[0].NameElement = new CdiServices.Registration.OrganisationNameTypeNameElement[1];
                party.PartyName.OrganisationName[0].NameElement[0] = new CdiServices.Registration.OrganisationNameTypeNameElement();
                party.PartyName.OrganisationName[0].NameElement[0].ElementTypeSpecified = true;
                party.PartyName.OrganisationName[0].NameElement[0].ElementType = CdiServices.Registration.OrganisationNameElementList.FullName;
                party.PartyName.OrganisationName[0].NameElement[0].Value = companyName; 

                }

                if(companyName != "")
                {
                    party.Documents = new CdiServices.Registration.PartyTypeDocuments();
                    party.Documents.Document = new CdiServices.Registration.PartyTypeDocumentsDocument[1];
                    party.Documents.Document[0] = new CdiServices.Registration.PartyTypeDocumentsDocument();
                    party.Documents.Document[0].DocumentElement = new CdiServices.Registration.PartyTypeDocumentsDocumentDocumentElement[1];
                    party.Documents.Document[0].DocumentElement[0] = new CdiServices.Registration.PartyTypeDocumentsDocumentDocumentElement();
                    party.Documents.Document[0].DocumentElement[0].Type = "Company";
                    party.Documents.Document[0].DocumentElement[0].Value = licence;
                }

                // Add the party object to the request
                confCurrRegPersonRequest.RequestBody[0].Party = party;

                // Step 2.3 - Create the soap header security
                CdiServices.Registration.Security registrationSecurity = new CdiServices.Registration.Security();

                // Set the SecurityTokenReference.Id previously returned by CDI
                // This holds the name/id of the CDI token e.g. 'cdiToken'
                registrationSecurity.SecurityTokenReference = new CdiServices.Registration.SecurityTokenReference();
                registrationSecurity.SecurityTokenReference.Id = cdiTokenId;

                // Set the CDIToken previously returned by CDI
                registrationSecurity.CDIToken = new CdiServices.Registration.CDIToken();
                registrationSecurity.CDIToken.Id = cdiTokenId;
                registrationSecurity.CDIToken.TokenValue = cdiTokenValue;
                registrationSecurity.CDIToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];

                // Set the CDISessionToken
                // This is an empty string until CDI returns a value for this in the response from this web method call
                registrationSecurity.CDISessionToken = cdiSessionToken;

                // Create a client identifier object containing a MotoChek client id
                registrationSecurity.ClientIdentifiers = new CdiServices.Registration.ClientIdentifiersType();
                registrationSecurity.ClientIdentifiers.AccountId = new CdiServices.Registration.ClientIdentifiersTypeAccountId();
                registrationSecurity.ClientIdentifiers.AccountId.AccountTypeSpecified = true;
                registrationSecurity.ClientIdentifiers.AccountId.AccountType = CdiClient.CdiServices.Registration.ClientIdentifiersTypeAccountIdAccountType.MotoChek;
                registrationSecurity.ClientIdentifiers.AccountId.Value = ConfigurationManager.AppSettings["ConfirmCurrentRegisteredPerson_AccountID"];

                // Set the soap header security
                registrationService.SecurityValue = registrationSecurity;

                // Step 2.4 - Call the ConfirmCurrentRegisteredPerson web method
                CdiServices.Registration.ConfirmCurrentRegisteredPersonResult confCurrRegPersonResponse = null;
                confCurrRegPersonResponse = registrationService.ConfirmCurrentRegisteredPerson(confCurrRegPersonRequest);

                // Step 2.5 - Store the CDISessionToken returned from CDI
                // This is used by CDI to identify a client session
                if (registrationService.SecurityValue != null && registrationService.SecurityValue.CDISessionToken != null)
                {
                    cdiSessionToken = registrationService.SecurityValue.CDISessionToken;
                    Console.WriteLine("CDISessionToken '{0}' returned from CDI", cdiSessionToken);
                }

                // Step 2.6 - Check for messages returned from CDI
                if (confCurrRegPersonResponse.ServiceHeader != null && confCurrRegPersonResponse.ServiceHeader.Status != null && confCurrRegPersonResponse.ServiceHeader.Status.Messages != null &&
                    confCurrRegPersonResponse.ServiceHeader.Status.Messages.Count() > 0)
                {
                    foreach (CdiServices.Registration.MessageType messageType in confCurrRegPersonResponse.ServiceHeader.Status.Messages)
                    {
                        Console.WriteLine("Message returned from CDI:");
                        Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                            messageType.Code, messageType.Value);
                    }
                }

                // Step 2.7 - Check for an error code returned from CDI
                if (confCurrRegPersonResponse != null && confCurrRegPersonResponse.ServiceHeader != null && confCurrRegPersonResponse.ServiceHeader.Status != null &&
                    confCurrRegPersonResponse.ServiceHeader.Status.Code != null && confCurrRegPersonResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
                {
                    Console.WriteLine("CDI ConfirmCurrentRegisteredPerson web method completed ok");
                }
                else
                {
                    cdiError = true;
                    Console.WriteLine("CDI ConfirmCurrentRegisteredPerson web method did not complete ok");
                }


                // Step 2.8 - Retrieve the response data
                CdiServices.Registration.ArrayOfConfirmCurrentRegisteredPersonResponseResponseDetailResponseDetail[] responseDetail = confCurrRegPersonResponse.ResponseBody;

                if (responseDetail.Count() > 0)
                {
                    foreach (CdiServices.Registration.ArrayOfConfirmCurrentRegisteredPersonResponseResponseDetailResponseDetail reqType in responseDetail)
                    {
                        if (reqType.ResponseTypeSpecified == true)
                        {
                            objReturn = reqType;
                        }
                    }
                }


                Console.WriteLine("- end -");
                Console.WriteLine();
            }

            #endregion

            #region Step 3.0 - Call the CDI EndSession web method

            // Step 3.1 - Create the AdministrationService web service proxy
            Console.WriteLine("Calling the CDI AdministrationService web service and EndSession web method ...");
            Console.WriteLine("- start -");
            CdiServices.AdministrationService.AdministrationService adminService = new CdiServices.AdministrationService.AdministrationService();


            // Step 3.2 - Create the request for the EndSession web method
            CdiServices.AdministrationService.EndSessionRequest endSessionRequest = new CdiServices.AdministrationService.EndSessionRequest();

            // Step 3.3 - Create the soap header security
            CdiServices.AdministrationService.Security adminServiceSecurity = new CdiServices.AdministrationService.Security();

            // Set the SecurityTokenReference.Id previously returned by CDI
            adminServiceSecurity.SecurityTokenReference = new CdiServices.AdministrationService.SecurityTokenReference();
            adminServiceSecurity.SecurityTokenReference.Id = cdiTokenId;

            // Set the CDIToken previously returned by CDI
            adminServiceSecurity.CDIToken = new CdiServices.AdministrationService.CDIToken();
            adminServiceSecurity.CDIToken.Id = cdiTokenId;
            adminServiceSecurity.CDIToken.TokenValue = cdiTokenValue;
            adminServiceSecurity.CDIToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];

            // Set the CDISessionToken previously returned by CDI
            adminServiceSecurity.CDISessionToken = cdiSessionToken;

            // Set the soap security header
            adminService.SecurityValue = adminServiceSecurity;

            // Step 3.4 - Call the EndSession web method
            CdiServices.AdministrationService.EndSessionResponse endSessionResponse = null;
            endSessionResponse = adminService.EndSession(endSessionRequest);

            // Step 3.5 - Check for messages returned from CDI
            if (endSessionResponse.ServiceHeader != null && endSessionResponse.ServiceHeader.Status != null && endSessionResponse.ServiceHeader.Status.Messages != null &&
                endSessionResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.AdministrationService.MessageType messageType in endSessionResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 3.6 - Check for an error code returned from CDI
            if (endSessionResponse != null && endSessionResponse.ServiceHeader != null && endSessionResponse.ServiceHeader.Status != null && endSessionResponse.ServiceHeader.Status.Code != null &&
                endSessionResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI EndSession web method completed ok");
            }
            else
            {
                Console.WriteLine("CDI EndSession web method did not complete ok");
            }

            return objReturn;

            #endregion

        }


        public static CdiServices.Registration.ArrayOfInquireCurrentDetailsResponseResponseDetailResponseDetail getCurrentDetailsVehicle(string rego)
        {
            Console.WriteLine();
            Console.WriteLine("Calling the CDI Confirm Current Registered Person transaction");
            Console.WriteLine();

            #region Local vars

            // These vars are used to retain values returned from CDI responses
            string cdiTokenId = null;               // The name of the CDIToken returned from CDI e.g. 'cdiToken'
            string cdiTokenValue = null;            // The value of the CDIToken returned from CDI e.g. '58be816a-8b84-4ab7-bdfd-20f2a946ff83'
            string cdiSessionToken = string.Empty;  // The value of the CDISessionToken returned from CDI e.g. '63ab802b-8b84-5ab7-bdgh-22f2a947ff82'
            bool cdiError = false;                  // Indicates that a CDI error was returned

            #endregion

            #region Step 1.0 - Call the CDI AuthenticateClient web method

            // Step 1.1 - Create AccessControl web service proxy
            Console.WriteLine("Calling the CDI AccessControl web service and AuthenticateClient web method");
            Console.WriteLine("- start -");
            CdiServices.AccessControl.AccessControl accessControlService = new CdiServices.AccessControl.AccessControl();

            // Step 1.2 - Create the request for the AuthenticateClient web method
            // The AuthenticateClient web method only requires an empty request to be input
            CdiServices.AccessControl.AuthenticateClientRequest authClientRequest = new CdiServices.AccessControl.AuthenticateClientRequest();
            authClientRequest.RequestBody = new object();

            // Step 1.3 - Create the soap header security

            // The UserNameToken is required by CDI to identify the client
            // CDI returns a CDIToken that uniquely identifies the client
            // The CDIToken must be submitted by the client with each subsequent request to CDI
            CdiServices.AccessControl.Security accessControlSecurity = new CdiServices.AccessControl.Security();
            accessControlSecurity.UserNameToken = new CdiServices.AccessControl.UserNameToken();
            accessControlSecurity.UserNameToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];     // The user name of the CDI client
            accessControlSecurity.UserNameToken.Password = ConfigurationManager.AppSettings["CdiPassword"];     // The password of the CDI client
            // Note: accessControlSecurity.UserNameToken.GroupName is optional

            // Set the soap security header
            accessControlService.SecurityValue = accessControlSecurity;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.SetTcpKeepAlive(false, 1000, 1000);
            // Step 1.4 - Call the AuthenticateClient web method
            CdiServices.AccessControl.AuthenticateClientResponse authClientResponse = null;
            authClientResponse = accessControlService.AuthenticateClient(authClientRequest);

            // Step 1.5 - Retain the CDIToken.Id and CDIToken.TokenValue returned from CDI
            // These need to be submitted with each subsequent web method call to CDI
            if (accessControlService.SecurityValue != null && accessControlService.SecurityValue.CDIToken != null && accessControlService.SecurityValue.CDIToken.Id != null)
            {
                cdiTokenId = accessControlService.SecurityValue.CDIToken.Id;
                Console.WriteLine("CDIToken.Id '{0}' returned from CDI", cdiTokenId);
            }

            if (accessControlService.SecurityValue != null && accessControlService.SecurityValue.CDIToken != null && accessControlService.SecurityValue.CDIToken.TokenValue != null)
            {
                cdiTokenValue = accessControlService.SecurityValue.CDIToken.TokenValue;
                Console.WriteLine("CDIToken.TokenValue {0} returned from CDI", cdiTokenValue);
            }

            // Step 1.6 - Retain the CDISessionToken returned from CDI
            // Thes needs to be submitted with each subsequent web method call to CDI
            if (accessControlSecurity != null && accessControlSecurity.CDISessionToken != null)
            {
                cdiSessionToken = accessControlService.SecurityValue.CDISessionToken;
                Console.WriteLine("CDISessionToken '{0}' returned from CDI", cdiSessionToken);
            }

            // Step 1.7 - Check for messages returned from CDI
            if (authClientResponse.ServiceHeader != null && authClientResponse.ServiceHeader.Status != null && authClientResponse.ServiceHeader.Status.Messages != null &&
                authClientResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.AccessControl.MessageType messageType in authClientResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 1.8 - Check for an error code returned from CDI
            if (authClientResponse != null && authClientResponse.ServiceHeader != null && authClientResponse.ServiceHeader.Status != null &&
                authClientResponse.ServiceHeader.Status.Code != null && authClientResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI AuthenticateClient web method completed ok");
            }
            else
            {
                cdiError = true;
                Console.WriteLine("CDI AuthenticateClient web method did not complete ok");
            }

            Console.WriteLine("- end -");
            Console.WriteLine();

            #endregion

            #region testVehicle
            //CdiServices.Vehicle.Vehicle vehicle = new CdiServices.Vehicle.Vehicle();

            CdiServices.Registration.Registration vehicle = new CdiServices.Registration.Registration();


            CdiServices.Registration.InquireCurrentDetailsRequest inquireVehicleDetailsRequest = new CdiServices.Registration.InquireCurrentDetailsRequest();
            inquireVehicleDetailsRequest.RequestBody = new CdiServices.Registration.VehicleOrPlateType[1];
            inquireVehicleDetailsRequest.RequestBody[0] = new CdiServices.Registration.VehicleOrPlateType();

            CdiServices.Registration.BasicPlateType basicPlate1 = new CdiClient.CdiServices.Registration.BasicPlateType();
            basicPlate1.PlateNumber = rego;
            inquireVehicleDetailsRequest.RequestBody[0].Item = basicPlate1;      // Add the plate to the request
                                                                                 // Step 2.3 - Create the soap header security
            CdiServices.Registration.Security registrationSecurity2 = new CdiServices.Registration.Security();

            // Set the SecurityTokenReference.Id previously returned by CDI
            // This holds the name/id of the CDI token e.g. 'cdiToken'
            registrationSecurity2.SecurityTokenReference = new CdiServices.Registration.SecurityTokenReference();
            registrationSecurity2.SecurityTokenReference.Id = cdiTokenId;

            // Set the CDIToken previously returned by CDI
            registrationSecurity2.CDIToken = new CdiServices.Registration.CDIToken();
            registrationSecurity2.CDIToken.Id = cdiTokenId;
            registrationSecurity2.CDIToken.TokenValue = cdiTokenValue;
            registrationSecurity2.CDIToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];

            // Set the CDISessionToken
            // This is an empty string until CDI returns a value for this in the response from this web method call
            registrationSecurity2.CDISessionToken = cdiSessionToken;

            // Create a client identifier object containing a MotoChek client id
            registrationSecurity2.ClientIdentifiers = new CdiServices.Registration.ClientIdentifiersType();
            registrationSecurity2.ClientIdentifiers.AccountId = new CdiServices.Registration.ClientIdentifiersTypeAccountId();
            registrationSecurity2.ClientIdentifiers.AccountId.AccountTypeSpecified = true;
            registrationSecurity2.ClientIdentifiers.AccountId.AccountType = CdiClient.CdiServices.Registration.ClientIdentifiersTypeAccountIdAccountType.MotoChek;
            registrationSecurity2.ClientIdentifiers.AccountId.Value = ConfigurationManager.AppSettings["ConfirmCurrentRegisteredPerson_AccountID"];

            // Set the soap header security
            vehicle.SecurityValue = registrationSecurity2;

            // Step 2.4 - Call the ConfirmCurrentRegisteredPerson web method
            CdiServices.Registration.InquireCurrentDetailsResult confCurrRegPersonResponse = null;
            confCurrRegPersonResponse = vehicle.InquireCurrentDetails(inquireVehicleDetailsRequest);


            // Step 2.5 - Store the CDISessionToken returned from CDI
            // This is used by CDI to identify a client session
            if (vehicle.SecurityValue != null && vehicle.SecurityValue.CDISessionToken != null)
            {
                cdiSessionToken = vehicle.SecurityValue.CDISessionToken;
                Console.WriteLine("CDISessionToken '{0}' returned from CDI", cdiSessionToken);
            }

            // Step 2.6 - Check for messages returned from CDI
            if (confCurrRegPersonResponse.ServiceHeader != null && confCurrRegPersonResponse.ServiceHeader.Status != null && confCurrRegPersonResponse.ServiceHeader.Status.Messages != null &&
                confCurrRegPersonResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.Registration.MessageType messageType in confCurrRegPersonResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 2.7 - Check for an error code returned from CDI
            if (confCurrRegPersonResponse != null && confCurrRegPersonResponse.ServiceHeader != null && confCurrRegPersonResponse.ServiceHeader.Status != null &&
                confCurrRegPersonResponse.ServiceHeader.Status.Code != null && confCurrRegPersonResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI ConfirmCurrentRegisteredPerson web method completed ok");
            }
            else
            {
                cdiError = true;
                Console.WriteLine("CDI ConfirmCurrentRegisteredPerson web method did not complete ok");
            }


            #endregion

            #region Step 3.0 - Call the CDI EndSession web method

            // Step 3.1 - Create the AdministrationService web service proxy
            Console.WriteLine("Calling the CDI AdministrationService web service and EndSession web method ...");
            Console.WriteLine("- start -");
            CdiServices.AdministrationService.AdministrationService adminService = new CdiServices.AdministrationService.AdministrationService();


            // Step 3.2 - Create the request for the EndSession web method
            CdiServices.AdministrationService.EndSessionRequest endSessionRequest = new CdiServices.AdministrationService.EndSessionRequest();

            // Step 3.3 - Create the soap header security
            CdiServices.AdministrationService.Security adminServiceSecurity = new CdiServices.AdministrationService.Security();

            // Set the SecurityTokenReference.Id previously returned by CDI
            adminServiceSecurity.SecurityTokenReference = new CdiServices.AdministrationService.SecurityTokenReference();
            adminServiceSecurity.SecurityTokenReference.Id = cdiTokenId;

            // Set the CDIToken previously returned by CDI
            adminServiceSecurity.CDIToken = new CdiServices.AdministrationService.CDIToken();
            adminServiceSecurity.CDIToken.Id = cdiTokenId;
            adminServiceSecurity.CDIToken.TokenValue = cdiTokenValue;
            adminServiceSecurity.CDIToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];

            // Set the CDISessionToken previously returned by CDI
            adminServiceSecurity.CDISessionToken = cdiSessionToken;

            // Set the soap security header
            adminService.SecurityValue = adminServiceSecurity;

            // Step 3.4 - Call the EndSession web method
            CdiServices.AdministrationService.EndSessionResponse endSessionResponse = null;
            endSessionResponse = adminService.EndSession(endSessionRequest);

            // Step 3.5 - Check for messages returned from CDI
            if (endSessionResponse.ServiceHeader != null && endSessionResponse.ServiceHeader.Status != null && endSessionResponse.ServiceHeader.Status.Messages != null &&
                endSessionResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.AdministrationService.MessageType messageType in endSessionResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 3.6 - Check for an error code returned from CDI
            if (endSessionResponse != null && endSessionResponse.ServiceHeader != null && endSessionResponse.ServiceHeader.Status != null && endSessionResponse.ServiceHeader.Status.Code != null &&
                endSessionResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI EndSession web method completed ok");
            }
            else
            {
                Console.WriteLine("CDI EndSession web method did not complete ok");
            }
            // Step 2.8 - Retrieve the response data
            CdiServices.Registration.ArrayOfInquireCurrentDetailsResponseResponseDetailResponseDetail[] responseDetail = confCurrRegPersonResponse.ResponseBody;
            CdiServices.Registration.ArrayOfInquireCurrentDetailsResponseResponseDetailResponseDetail objReturn = new CdiServices.Registration.ArrayOfInquireCurrentDetailsResponseResponseDetailResponseDetail();


            if (responseDetail.Count() > 0)
            {
                foreach (CdiServices.Registration.ArrayOfInquireCurrentDetailsResponseResponseDetailResponseDetail reqType in responseDetail)
                {
                    if (reqType.ResponseTypeSpecified == true)
                    {
                        objReturn = reqType;
                    }
                }
            }

            return objReturn;


            #endregion

        }


        public static CdiServices.Registration.ArrayOfInquireRegisteredPersonHistoryResponseResponseDetailResponseDetail getOwnerHistory(string rego)
        {
            Console.WriteLine();
            Console.WriteLine("Calling the CDI Confirm Current Registered Person transaction");
            Console.WriteLine();
            CdiServices.Registration.ArrayOfInquireRegisteredPersonHistoryResponseResponseDetailResponseDetail objReturn = new CdiServices.Registration.ArrayOfInquireRegisteredPersonHistoryResponseResponseDetailResponseDetail();
            #region Local vars

            // These vars are used to retain values returned from CDI responses
            string cdiTokenId = null;               // The name of the CDIToken returned from CDI e.g. 'cdiToken'
            string cdiTokenValue = null;            // The value of the CDIToken returned from CDI e.g. '58be816a-8b84-4ab7-bdfd-20f2a946ff83'
            string cdiSessionToken = string.Empty;  // The value of the CDISessionToken returned from CDI e.g. '63ab802b-8b84-5ab7-bdgh-22f2a947ff82'
            bool cdiError = false;                  // Indicates that a CDI error was returned

            #endregion

            #region Step 1.0 - Call the CDI AuthenticateClient web method

            // Step 1.1 - Create AccessControl web service proxy
            Console.WriteLine("Calling the CDI AccessControl web service and AuthenticateClient web method");
            Console.WriteLine("- start -");
            CdiServices.AccessControl.AccessControl accessControlService = new CdiServices.AccessControl.AccessControl();

            // Step 1.2 - Create the request for the AuthenticateClient web method
            // The AuthenticateClient web method only requires an empty request to be input
            CdiServices.AccessControl.AuthenticateClientRequest authClientRequest = new CdiServices.AccessControl.AuthenticateClientRequest();
            authClientRequest.RequestBody = new object();

            // Step 1.3 - Create the soap header security

            // The UserNameToken is required by CDI to identify the client
            // CDI returns a CDIToken that uniquely identifies the client
            // The CDIToken must be submitted by the client with each subsequent request to CDI
            CdiServices.AccessControl.Security accessControlSecurity = new CdiServices.AccessControl.Security();
            accessControlSecurity.UserNameToken = new CdiServices.AccessControl.UserNameToken();
            accessControlSecurity.UserNameToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];     // The user name of the CDI client
            accessControlSecurity.UserNameToken.Password = ConfigurationManager.AppSettings["CdiPassword"];     // The password of the CDI client
                                                                                                                // Note: accessControlSecurity.UserNameToken.GroupName is optional

            // Set the soap security header
            accessControlService.SecurityValue = accessControlSecurity;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.SetTcpKeepAlive(false, 1000, 1000);

            // Step 1.4 - Call the AuthenticateClient web method
            CdiServices.AccessControl.AuthenticateClientResponse authClientResponse = null;
            authClientResponse = accessControlService.AuthenticateClient(authClientRequest);

            // Step 1.5 - Retain the CDIToken.Id and CDIToken.TokenValue returned from CDI
            // These need to be submitted with each subsequent web method call to CDI
            if (accessControlService.SecurityValue != null && accessControlService.SecurityValue.CDIToken != null && accessControlService.SecurityValue.CDIToken.Id != null)
            {
                cdiTokenId = accessControlService.SecurityValue.CDIToken.Id;
                Console.WriteLine("CDIToken.Id '{0}' returned from CDI", cdiTokenId);
            }
            if (accessControlService.SecurityValue != null && accessControlService.SecurityValue.CDIToken != null && accessControlService.SecurityValue.CDIToken.TokenValue != null)
            {
                cdiTokenValue = accessControlService.SecurityValue.CDIToken.TokenValue;
                Console.WriteLine("CDIToken.TokenValue {0} returned from CDI", cdiTokenValue);
            }

            // Step 1.6 - Retain the CDISessionToken returned from CDI
            // Thes needs to be submitted with each subsequent web method call to CDI
            if (accessControlSecurity != null && accessControlSecurity.CDISessionToken != null)
            {
                cdiSessionToken = accessControlService.SecurityValue.CDISessionToken;
                Console.WriteLine("CDISessionToken '{0}' returned from CDI", cdiSessionToken);
            }

            // Step 1.7 - Check for messages returned from CDI
            if (authClientResponse.ServiceHeader != null && authClientResponse.ServiceHeader.Status != null && authClientResponse.ServiceHeader.Status.Messages != null &&
                authClientResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.AccessControl.MessageType messageType in authClientResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 1.8 - Check for an error code returned from CDI
            if (authClientResponse != null && authClientResponse.ServiceHeader != null && authClientResponse.ServiceHeader.Status != null &&
                authClientResponse.ServiceHeader.Status.Code != null && authClientResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI AuthenticateClient web method completed ok");
            }
            else
            {
                cdiError = true;
                Console.WriteLine("CDI AuthenticateClient web method did not complete ok");
            }

            Console.WriteLine("- end -");
            Console.WriteLine();

            #endregion

            #region Step 2.0 - Call the CDI ConfirmCurrentRegisteredPerson web method

            if (cdiError == false)
            {
                // Step 2.1 - Create the ConfirmCurrentRegisteredPerson web service proxy
                Console.WriteLine("Calling the CDI Registration web service and ConfirmCurrentRegisteredPerson web method ...");
                Console.WriteLine("- start -");
                CdiServices.Registration.Registration registrationService = new CdiServices.Registration.Registration();

                // Step 2.2 - Create the request for the ConfirmCurrentRegisteredPerson web method
                // Step 2.2 - Create the request for the ConfirmCurrentRegisteredPerson web method
                CdiServices.Registration.InquireRegisteredPersonHistoryRequest confCurrRegPersonRequest = new CdiClient.CdiServices.Registration.InquireRegisteredPersonHistoryRequest();
                confCurrRegPersonRequest.RequestBody = new CdiServices.Registration.VehicleOrPlateType[1];
                confCurrRegPersonRequest.RequestBody[0] = new CdiServices.Registration.VehicleOrPlateType();
                confCurrRegPersonRequest.RequestBody[0].Item = new CdiServices.Registration.VehicleOrPlateType();

                // Create a party object
                CdiServices.Registration.PartyType party = new CdiServices.Registration.PartyType();
                party.PartyType1Specified = true;
                party.PartyType1 = CdiClient.CdiServices.Registration.PartyTypeList.Individual;

                // Create a VehicleOrPlateRequestType that can hold either a BasicPlateType or a BasicVehicleDetailsType
                CdiServices.Registration.BasicPlateType basicPlate = new CdiClient.CdiServices.Registration.BasicPlateType();
                basicPlate.PlateNumber = rego;
                confCurrRegPersonRequest.RequestBody[0].Item = basicPlate;      // Add the plate to the request


                // Step 2.3 - Create the soap header security
                CdiServices.Registration.Security registrationSecurity = new CdiServices.Registration.Security();

                // Set the SecurityTokenReference.Id previously returned by CDI
                // This holds the name/id of the CDI token e.g. 'cdiToken'
                registrationSecurity.SecurityTokenReference = new CdiServices.Registration.SecurityTokenReference();
                registrationSecurity.SecurityTokenReference.Id = cdiTokenId;

                // Set the CDIToken previously returned by CDI
                registrationSecurity.CDIToken = new CdiServices.Registration.CDIToken();
                registrationSecurity.CDIToken.Id = cdiTokenId;
                registrationSecurity.CDIToken.TokenValue = cdiTokenValue;
                registrationSecurity.CDIToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];

                // Set the CDISessionToken
                // This is an empty string until CDI returns a value for this in the response from this web method call
                registrationSecurity.CDISessionToken = cdiSessionToken;

                // Create a client identifier object containing a MotoChek client id
                registrationSecurity.ClientIdentifiers = new CdiServices.Registration.ClientIdentifiersType();
                registrationSecurity.ClientIdentifiers.AccountId = new CdiServices.Registration.ClientIdentifiersTypeAccountId();
                registrationSecurity.ClientIdentifiers.AccountId.AccountTypeSpecified = true;
                registrationSecurity.ClientIdentifiers.AccountId.AccountType = CdiClient.CdiServices.Registration.ClientIdentifiersTypeAccountIdAccountType.MotoChek;
                registrationSecurity.ClientIdentifiers.AccountId.Value = ConfigurationManager.AppSettings["ConfirmCurrentRegisteredPerson_AccountID"];

                // Set the soap header security
                registrationService.SecurityValue = registrationSecurity;

                // Step 2.4 - Call the ConfirmCurrentRegisteredPerson web method
                CdiServices.Registration.InquireRegisteredPersonHistoryResult confCurrRegPersonResponse = null;
                confCurrRegPersonResponse = registrationService.InquireRegisteredPersonHistory(confCurrRegPersonRequest);

                // Step 2.5 - Store the CDISessionToken returned from CDI
                // This is used by CDI to identify a client session
                if (registrationService.SecurityValue != null && registrationService.SecurityValue.CDISessionToken != null)
                {
                    cdiSessionToken = registrationService.SecurityValue.CDISessionToken;
                    Console.WriteLine("CDISessionToken '{0}' returned from CDI", cdiSessionToken);
                }

                // Step 2.6 - Check for messages returned from CDI
                if (confCurrRegPersonResponse.ServiceHeader != null && confCurrRegPersonResponse.ServiceHeader.Status != null && confCurrRegPersonResponse.ServiceHeader.Status.Messages != null &&
                    confCurrRegPersonResponse.ServiceHeader.Status.Messages.Count() > 0)
                {
                    foreach (CdiServices.Registration.MessageType messageType in confCurrRegPersonResponse.ServiceHeader.Status.Messages)
                    {
                        Console.WriteLine("Message returned from CDI:");
                        Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                            messageType.Code, messageType.Value);
                    }
                }

                // Step 2.7 - Check for an error code returned from CDI
                if (confCurrRegPersonResponse != null && confCurrRegPersonResponse.ServiceHeader != null && confCurrRegPersonResponse.ServiceHeader.Status != null &&
                    confCurrRegPersonResponse.ServiceHeader.Status.Code != null && confCurrRegPersonResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
                {
                    Console.WriteLine("CDI ConfirmCurrentRegisteredPerson web method completed ok");
                }
                else
                {
                    cdiError = true;
                    Console.WriteLine("CDI ConfirmCurrentRegisteredPerson web method did not complete ok");
                }


                // Step 2.8 - Retrieve the response data
                CdiServices.Registration.ArrayOfInquireRegisteredPersonHistoryResponseResponseDetailResponseDetail[] responseDetail = confCurrRegPersonResponse.ResponseBody;

                if (responseDetail.Count() > 0)
                {
                    foreach (CdiServices.Registration.ArrayOfInquireRegisteredPersonHistoryResponseResponseDetailResponseDetail reqType in responseDetail)
                    {
                        if (reqType.ResponseTypeSpecified == true)
                        {
                            objReturn = reqType;
                        }
                    }
                }


                Console.WriteLine("- end -");
                Console.WriteLine();
            }

            #endregion

            #region Step 3.0 - Call the CDI EndSession web method

            // Step 3.1 - Create the AdministrationService web service proxy
            Console.WriteLine("Calling the CDI AdministrationService web service and EndSession web method ...");
            Console.WriteLine("- start -");
            CdiServices.AdministrationService.AdministrationService adminService = new CdiServices.AdministrationService.AdministrationService();


            // Step 3.2 - Create the request for the EndSession web method
            CdiServices.AdministrationService.EndSessionRequest endSessionRequest = new CdiServices.AdministrationService.EndSessionRequest();

            // Step 3.3 - Create the soap header security
            CdiServices.AdministrationService.Security adminServiceSecurity = new CdiServices.AdministrationService.Security();

            // Set the SecurityTokenReference.Id previously returned by CDI
            adminServiceSecurity.SecurityTokenReference = new CdiServices.AdministrationService.SecurityTokenReference();
            adminServiceSecurity.SecurityTokenReference.Id = cdiTokenId;

            // Set the CDIToken previously returned by CDI
            adminServiceSecurity.CDIToken = new CdiServices.AdministrationService.CDIToken();
            adminServiceSecurity.CDIToken.Id = cdiTokenId;
            adminServiceSecurity.CDIToken.TokenValue = cdiTokenValue;
            adminServiceSecurity.CDIToken.UserName = ConfigurationManager.AppSettings["CdiUserName"];

            // Set the CDISessionToken previously returned by CDI
            adminServiceSecurity.CDISessionToken = cdiSessionToken;

            // Set the soap security header
            adminService.SecurityValue = adminServiceSecurity;

            // Step 3.4 - Call the EndSession web method
            CdiServices.AdministrationService.EndSessionResponse endSessionResponse = null;
            endSessionResponse = adminService.EndSession(endSessionRequest);

            // Step 3.5 - Check for messages returned from CDI
            if (endSessionResponse.ServiceHeader != null && endSessionResponse.ServiceHeader.Status != null && endSessionResponse.ServiceHeader.Status.Messages != null &&
                endSessionResponse.ServiceHeader.Status.Messages.Count() > 0)
            {
                foreach (CdiServices.AdministrationService.MessageType messageType in endSessionResponse.ServiceHeader.Status.Messages)
                {
                    Console.WriteLine("Message returned from CDI:");
                    Console.WriteLine("   Message Origin = {0}, Message Type = {1}, Message Code = {2}, Message Value = {3}", messageType.Origin, messageType.Type,
                        messageType.Code, messageType.Value);
                }
            }

            // Step 3.6 - Check for an error code returned from CDI
            if (endSessionResponse != null && endSessionResponse.ServiceHeader != null && endSessionResponse.ServiceHeader.Status != null && endSessionResponse.ServiceHeader.Status.Code != null &&
                endSessionResponse.ServiceHeader.Status.Code == Constants.CdiStatus102)
            {
                Console.WriteLine("CDI EndSession web method completed ok");
            }
            else
            {
                Console.WriteLine("CDI EndSession web method did not complete ok");
            }

            return objReturn;

            #endregion

        }





    }
}
