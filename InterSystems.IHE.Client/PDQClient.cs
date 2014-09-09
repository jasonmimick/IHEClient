using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using System.Dynamic;
using InterSystems.IHE.Client.IHE.PDQ;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace InterSystems.IHE.Client
{
   
    public class PDQClient : IHEClient
    {
		public PDQClient() 
		{
			this.Verbose = false;
			
		}
		public static string TestingEndpoint = 
             "http://exchange.healthshare.us:57772/csp/healthshare/hsregistry/HS.IHE.PDQv3.Supplier.Services.CLS";

        PRPA_IN201305UV02 getQuery(PDQRequest user_query)
        {
            var query = new PRPA_IN201305UV02();

            query.id = new II() { extension = "1", root = "9E20903A-2A16-11E4-B010-313907842300" };
            query.creationTime = new TS() { value = "20140822160855+0000" };

            query.interactionId = new II() { extension = "PRPA_IN201305UV02", root = "1.2.16.840.1.113883.1.6" };
            query.processingCode = new CS() { code = "T" };
            query.processingModeCode = new CS() { code = "T" };
            query.acceptAckCode = new CS() { code = "AL" };

            query.receiver = new MCCI_MT000100UV01Receiver[1];
            query.receiver[0] = new MCCI_MT000100UV01Receiver()
            {
                typeCode = CommunicationFunctionType.RCV
            };

            query.receiver[0].device = new MCCI_MT000100UV01Device()
            {
                classCode = EntityClassDevice.DEV,
                determinerCode = "INSTANCE"
            };


            query.sender = new MCCI_MT000100UV01Sender()
            {
                typeCode = CommunicationFunctionType.SND,
                device = new MCCI_MT000100UV01Device()
                {
                    classCode = EntityClassDevice.DEV,
                    determinerCode = "INSTANCE"
                }
            };
            query.sender.device.id = new II[1];
            query.sender.device.id[0] = new II() { root = "PDQv3.Sender" };

            query.controlActProcess = new PRPA_IN201305UV02QUQI_MT021001UV01ControlActProcess()
            {
                classCode = ActClassControlAct.CACT,
                moodCode = x_ActMoodIntentEvent.EVN
            };
            query.controlActProcess.code = new CD() { code = "PRPA_TE201305UV02", codeSystem = "2.16.840.1.113883.1.6" };

            var qp = new PRPA_MT201306UV02QueryByParameter();
            qp.queryId = new II() { root = "9E20901C-2A16-11E4-B010-313907842300" };
            qp.statusCode = new CS() { code = "new" };
            qp.responseModalityCode = new CS() { code = "R" };
            qp.responsePriorityCode = new CS() { code = "I" };
            qp.initialQuantity = new INT() { value = "999" };
            qp.parameterList = new PRPA_MT201306UV02ParameterList();
            /**/
            qp.parameterList.livingSubjectName = new PRPA_MT201306UV02LivingSubjectName[1];
            qp.parameterList.livingSubjectName[0] = new PRPA_MT201306UV02LivingSubjectName();

            qp.parameterList.livingSubjectName[0].value = new EN[1];
            qp.parameterList.livingSubjectName[0].value[0] = new EN { use = new String[] { "SRCH" } };

            var items = new List<ENXP>();
			trace("user_query="+user_query);
			if ( ( user_query.LastName != null ) && ( user_query.LastName != "") ) {
                items.Add(new enfamily { Text = new String[] { user_query.LastName } });
            }
			if ( ( user_query.FirstName != null ) && (user_query.FirstName != "") ) {
                items.Add(new engiven { Text = new String[] { user_query.FirstName } });
            }
            qp.parameterList.livingSubjectName[0].value[0].Items = items.ToArray<ENXP>();
            qp.parameterList.livingSubjectName[0].semanticsText = new ST() { Text = new String[] { "LivingSubject.name" } };

            /* NOT SUPPORTING DOB SEARCH
            if (user_query.GetType().GetProperty("DateOfBirth") != null)
            {
                qp.parameterList.livingSubjectBirthTime = new PRPA_MT201306UV02LivingSubjectBirthTime[1];
                var dob = new PRPA_MT201306UV02LivingSubjectBirthTime();
                dob.value = new IVL_TS[1];
                dob.value[0] = new IVL_TS() { value = user_query.DateOfBirth };
                qp.parameterList.livingSubjectBirthTime[0] = dob;
            }
            **/

            /**/
            query.controlActProcess.queryByParameter = qp;
            return query;
        }
        
        public List<PDQResponse> SearchExchange(PDQRequest query)
        {
			
            trace("SearchExchange() query = " + query);
            return ExecuteSearch(query);
        }

        /// <summary>
        /// Execute a Soap WebService call
        /// </summary>
        private List<PDQResponse> ExecuteSearch(PDQRequest user_query)
        {
			validate(user_query);
			var url = this.ExchangeEndpoint;
            var soap_action = "urn:hl7-org:v3:PRPA_IN201305UV02";
            HttpWebRequest request = CreateWebRequest(url, soap_action, user_query);
            XmlDocument soapEnvelopeXml = new XmlDocument();
            var query = getQuery(user_query);
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(PRPA_IN201305UV02), "urn:hl7-org:v3");
            var querySB = new StringBuilder();
            var xmlWriter = System.Xml.XmlWriter.Create(querySB,new XmlWriterSettings() { OmitXmlDeclaration = true });
            serializer.Serialize(xmlWriter, query);
            var queryXML = querySB.ToString();
			trace(queryXML);
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">";
            xml += @"  <soap:Header>
                <Security xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd""><UsernameToken>
                    <Username>";
			xml += user_query.WSSecurityUserName;
			xml += @"</Username>
                    <Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"">";
			xml += user_query.WSSecurityPassword;
			xml += @"</Password>
                    </UsernameToken></Security>  
                </soap:Header>";

            xml +="     <soap:Body>";
            xml += queryXML;
            xml += @"</soap:Body>
                </soap:Envelope>";
            soapEnvelopeXml.LoadXml(xml);
            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
            var simpleResults = new List<PDQResponse>();
			
              
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    var deserializer = new System.Xml.Serialization.XmlSerializer(typeof(PRPA_IN201306UV02), "urn:hl7-org:v3");
                    var re = XElement.Parse(soapResult);
                    re.Element("SOAP-ENV"+"Body");
                    var content = re.Elements().First().Elements().First();
					trace("content="+content);
                    var result = (PRPA_IN201306UV02) deserializer.Deserialize(content.CreateReader());
                    // for each result subject - fetch a patientPerson
                    // call method to get item for results to send back
                    //result.controlActProcess.subject.Select<PRPA_IN201306UV02MFMI_MT700711UV01Subject1>(subject => simpleResults.Add( getPatientInfo(subject.registrationEvent.subject1.patient));
                    if (result.controlActProcess.subject != null)
                    {
                        for (var i = 0; i < result.controlActProcess.subject.Length; i++)
                        {
                            var s = result.controlActProcess.subject[i];
                            simpleResults.Add(getPatientInfo(s.registrationEvent.subject1.patient));
                        }
                    }
                    //result.controlActProcess.subject.Select(s=>simpleResults.Add( getPatientInfo( s.registrationEvent.subject1.patient ));               
                }
            }
			trace("ExecuteSearch result count = "+simpleResults.Count);
            return simpleResults;
        }


        private static PDQResponse getPatientInfo(PRPA_MT201310UV02Patient patient)
        {
            var patientPerson = patient.Item;   
            //var patientPerson = ((PRPA_IN201306UV02MCCI_MT000300UV01Message)(result)).controlActProcess.subject[0].registrationEvent.subject1.patient.Item;
            var street = ((BIN)(((PRPA_MT201310UV02Person)(patientPerson)).addr[0].Items[0])).Text[0];
            var city = ((BIN)(((PRPA_MT201310UV02Person)(patientPerson)).addr[0].Items[1])).Text[0];
            var state = ((BIN)(((PRPA_MT201310UV02Person)(patientPerson)).addr[0].Items[2])).Text[0];
            var zip = ((BIN)(((PRPA_MT201310UV02Person)(patientPerson)).addr[0].Items[3])).Text[0];
            //var numberOtherIds = ((PRPA_MT201310UV02Person)(patientPerson)).asOtherIDs.Length;
            var ids = new List<PDQIdentifier>();
		
            for (var i = 0; i < patient.id.Length; i++ )
            {
                var id = patient.id[i];
                //ids.Add(new PDQIdentifier 
				//	{ ID = id.id[0].extension, Source = id.id[0].root });
                ids.Add(new PDQIdentifier 
					{ ID = id.extension, Source = id.root });
            }
         for (var i = 0; i < ((PRPA_MT201310UV02Person)(patientPerson)).asOtherIDs.Length; i++ )
            {
                var id = ((PRPA_MT201310UV02Person)(patientPerson)).asOtherIDs[i];
                //ids.Add(new { id = id.id[0].extension, source = id.id[0].root });
                ids.Add(new PDQIdentifier 
					{ ID = id.id[0].extension, Source = id.id[0].root });
            }
            //var ids = ((PRPA_MT201310UV02Person)(patientPerson)).asOtherIDs.Select(i => new { id = i.id[0].extension, source = i.id[0].root });
            var dob = ((PRPA_MT201310UV02Person)(patientPerson)).birthTime.value;
            var fname = ((EN)(((PRPA_MT201310UV02Person)(patientPerson)).name[0])).Items[0].Text[0];
            var lname = ((EN)(((PRPA_MT201310UV02Person)(patientPerson)).name[0])).Items[1].Text[0];
            var pdq_response = new PDQResponse()
            {
                FirstName = fname,
                LastName = lname,
                DateOfBirth = dob,
                Address = street+" "+city+","+state+" "+zip 
            };
			pdq_response.ids = ids;
			return pdq_response;
        }
       
        
       
        
        
        /// <summary>
        /// Create a soap webrequest to [Url]
        /// </summary>
        /// <returns></returns>
        //internal static HttpWebRequest CreateWebRequest(string url,string soap_action="")
        internal static HttpWebRequest CreateWebRequest(string url,
				string soap_action, IHERequest request)
        {
           
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            //@"http://exchange.healthshare.us:57772/csp/healthshare/hsregistry/%25SOAP.WebServiceInvoke.cls?CLS=HS.IHE.PDQv3.Supplier.Services&OP=Query");
           
           // webRequest.Headers.Add(@"SOAPAction","urn:hl7-org:v3:PRPA_IN201305UV02");
            if (soap_action.Length > 0)
            {
                webRequest.Headers.Add(@"SOAPAction", soap_action);
            }
               webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
			var u = request.HttpUserName; var p = request.HttpPassword;
            webRequest.Credentials = new NetworkCredential(u,p);

            ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
			/*
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback((new PDQClient()).delegateHttpSsl);
            */
            return webRequest;
        }


        private bool delegateHttpSsl(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
      
    }

}
