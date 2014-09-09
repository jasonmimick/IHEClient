using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using System.Dynamic;
using InterSystems.IHE.Client.IHE.XDSb;
using System.Security.Cryptography;
using System.Diagnostics;
namespace InterSystems.IHE.Client
{
    public  class MimeType
    {
        public static MimeType PDF = new MimeType("application/pdf");
        public static MimeType XML = new MimeType("text/xml");
        public static MimeType JPG = new MimeType("image/jpeg");
        private MimeType(string mime)
        {
            this.mime = mime;
        }
        private string mime;
        public override string ToString()
        {
            return this.mime;
        }
    }

    public static class XDS_UUIDS
    {
        public static string XDSDocumentEntry = "urn:uuid:7edca82f-054d-47f2-a032-9b2a5b5186c1";
    }

  
   
    public class XDSbClient : IHEClient
    {
        private static SlotType1 getSlot(string name, string value)
        {
            var slot = new SlotType1() { name = name };
            slot.ValueList = new ValueListType();
            slot.ValueList.Value = new string[1] { value };
            return slot;
        }
        private static  InternationalStringType getIStringType(string value) 
        {
            var ist = new InternationalStringType();
            ist.LocalizedString = new LocalizedStringType[1];
            ist.LocalizedString[0] = new LocalizedStringType { value = value };
            return ist;

        }
  
        private static ProvideAndRegisterDocumentSetRequestType 
			getProvideAndRegisterRequest(XDSbRequest request)
        {
            if (!request.Validate())
            {
                throw new Exception("request was not valid!");
            }
            Trace.TraceInformation("getProvideAndRegisterRequest called");
            var xsdb_req = new ProvideAndRegisterDocumentSetRequestType();
            var extObjectID = "urn:uuid:" + Guid.NewGuid().ToString();
            xsdb_req.SubmitObjectsRequest = new SubmitObjectsRequest();
            //var regObjList = new List<IdentifiableType>();
            var regObjList = new List<IdentifiableType>();
            var extrinsicObject = new ExtrinsicObjectType();
            //extrinsicObject.id = "urn:uuid:acd0b09b-11bb-4141-b996-57925a4325d0";
            extrinsicObject.id = extObjectID; 
            extrinsicObject.mimeType = request.mimeType.ToString();// "text/xml";   // INPUT!
            extrinsicObject.objectType = XDS_UUIDS.XDSDocumentEntry; 
            // FAKE DOCUMENT HERE

            var doc = new ProvideAndRegisterDocumentSetRequestTypeDocument();
            doc.id = "urn:uuid:" + Guid.NewGuid();

            /*
            var docString = @"<?xml version='1.0'?>
                <foo>
                <Hello>HELLO DUDE! HERE IS SOME IHE SHIZZZNITZZZ</Hello>
                </foo>";
            var docBytes = Encoding.UTF8.GetBytes(docString);
            doc.Value = docBytes;
            */
            doc.Value = request.data;

            xsdb_req.Document = new ProvideAndRegisterDocumentSetRequestTypeDocument[1] { doc };


            //var docSize = docBytes.Length.ToString();
            var docSize = request.data.Length.ToString();
            
            var hash = "";
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                //var hashBytes = sha1.ComputeHash(docBytes);
                var hashBytes = sha1.ComputeHash(request.data);

                //hash =  Convert.ToBase64String(hashBytes);
                hash = BitConverter.ToString(hashBytes).ToLower().Replace("-","");
                Trace.TraceInformation("hash=" + hash);
                
            }
            var slotData = new List<dynamic>();
            //slotData.Add(new { k = "creationTime", v = "08/25/2014 20:03:51", cdata = false });
            slotData.Add(new { k = "creationTime", 
					v = request.creationDateTime.ToUniversalTime().ToString() });
            slotData.Add(new { k = "languageCode", v = request.options.language/*"EN"*/ });
            //slotData.Add(new { k = "sourcePatientId", v = "100000001^^^&1.3.6.1.4.1.21367.2010.1.2.300&ISO", cdata = false });
            slotData.Add(new { k = "sourcePatientId", v = request.formattedIdentifier });
            slotData.Add(new { k = "size", v = docSize });
            slotData.Add(new { k = "hash", v = hash});


            // rim slots for ext
            var slots = new List<SlotType1>();
            slotData.ForEach(ds => slots.Add(getSlot(ds.k, (String)ds.v )));
            extrinsicObject.Slot = slots.ToArray();
			Trace.TraceInformation("slots done");
            // rim:Classifications for ext
            var classifications = new List<ClassificationType>();
            
            var c1 = new ClassificationType();
            c1.classificationScheme = "urn:uuid:41a5887f-8865-4c09-adf7-e362475b143a";
            c1.classifiedObject = extObjectID/* "urn:uuid:acd0b09b-11bb-4141-b996-57925a4325d0"*/;
            c1.id = "ClassCode1";
            //c1.nodeRepresentation = "11488-4";
            c1.nodeRepresentation = request.options.classCode.code;

            c1.objectType = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:Classification";
            c1.Slot = new SlotType1[1] { getSlot("codingScheme",request.options.classCode.codingScheme ) };
            c1.Name = getIStringType(request.options.classCode.display);

            classifications.Add(c1);

            //
            classifications.Add( new ClassificationType()
            {
                classificationScheme = "urn:uuid:f4f85eac-e6cb-4883-b524-f2705394840f",
                classifiedObject = extObjectID/* "urn:uuid:acd0b09b-11bb-4141-b996-57925a4325d0"*/,
                id = "ConfidentialityCode1",
                nodeRepresentation = request.options.confidentialityCode.code,
                objectType = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:Classification",
                Slot = new SlotType1[1] { getSlot("codingScheme",
								request.options.confidentialityCode.codingScheme ) },
                Name = getIStringType(request.options.confidentialityCode.display)
            });

            //
            classifications.Add(new ClassificationType()
            {
                classificationScheme = "urn:uuid:a09d5840-386c-46f2-b5ad-9c3699a4309d",
                classifiedObject = extObjectID/* "urn:uuid:acd0b09b-11bb-4141-b996-57925a4325d0"*/,
                id = "FormatCode1",
                nodeRepresentation = request.options.formatCode.code,
                objectType = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:Classification",
                Slot = new SlotType1[1] { getSlot("codingScheme", 
					request.options.formatCode.codingScheme ) },
                Name = getIStringType(request.options.formatCode.display)
            });

            //
            classifications.Add(new ClassificationType()
            {
                classificationScheme = "urn:uuid:f33fb8ac-18af-42cc-ae0e-ed0b0bdb91e1",
                classifiedObject = extObjectID/* "urn:uuid:acd0b09b-11bb-4141-b996-57925a4325d0"*/,
                id = "HealthcareFacilityTypeCode1",
                nodeRepresentation = request.options.healthcareFacilityTypeCode.code,
                Slot = new SlotType1[1] { getSlot("codingScheme", 
						request.options.healthcareFacilityTypeCode.codingScheme ) },
                Name = getIStringType(request.options.healthcareFacilityTypeCode.display)
            });
            
            //
            classifications.Add(new ClassificationType()
            {
                classificationScheme = "urn:uuid:cccf5598-8b07-4b77-a05e-ae952c785ead",
                classifiedObject = extObjectID/* "urn:uuid:acd0b09b-11bb-4141-b996-57925a4325d0"*/,
                id = "PracticeSettingCode",
                nodeRepresentation = request.options.practiceSettingCode.code,
                objectType = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:Classification",
                Slot = new SlotType1[1] { getSlot("codingScheme",
					request.options.practiceSettingCode.codingScheme ) },
                Name = getIStringType(request.options.practiceSettingCode.display)
            });

            classifications.Add(new ClassificationType()
            {
                classificationScheme = "urn:uuid:f0306f51-975f-434e-a61c-c59651d33983",
                classifiedObject = extObjectID/*"urn:uuid:9029ab80-3681-4348-8550-aaacaa5a9c2a"*/,
                id = "TypeCode1",
                nodeRepresentation = request.options.typeCode.code,
                objectType = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:Classification",
                Slot = new SlotType1[1] { getSlot("codingScheme",
						request.options.typeCode.codingScheme ) },
                Name = getIStringType( request.options.typeCode.display )
            }
            );
            
            extrinsicObject.Classification = classifications.ToArray();
            // end classifications
            
            // external ids--
            var externalIds = new List<ExternalIdentifierType>();
            externalIds.Add(new ExternalIdentifierType()
            {
                id = "id1",
                identificationScheme = "urn:uuid:58a6f841-87b3-4a3e-92fd-a8ffeff98427",
                objectType = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:ExternalIdentifier",
                registryObject = extObjectID/* "urn:uuid:9029ab80-3681-4348-8550-aaacaa5a9c2a"*/,
                value = request.formattedIdentifier,
                Name = getIStringType("XDSDocumentEntry.patientId")
            });

			/// *** Should we loop adding all the IDs we have for the patient???

            /*
            externalIds.Add(new ExternalIdentifierType()
            {
                id = "id2",
                identificationScheme = "urn:uuid:2e82c1f6-a085-4c72-9da3-8640a32e42ab",
                objectType = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:ExternalIdentifier",
                registryObject = extObjectID,
                value = "2.25.229710795011523829152619624891901224400",
                Name = getIStringType("XDSDocumentEntry.patientId")
            });
            */
			var XDSDocumentEntry_uniqueId = Guid.NewGuid().ToString();
            externalIds.Add(new ExternalIdentifierType()
            {
                id = "id4",
                identificationScheme = "urn:uuid:2e82c1f6-a085-4c72-9da3-8640a32e42ab"/*urn:uuid:96fdda7c-d067-4183-912e-bf5ee74998a8"*/,
                objectType = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:ExternalIdentifier",
                registryObject = extObjectID,
                value = XDSDocumentEntry_uniqueId,
                Name = getIStringType("XDSDocumentEntry.uniqueId")
            });
            extrinsicObject.ExternalIdentifier  = externalIds.ToArray();
            
            regObjList.Add(extrinsicObject);
            // end extrinsic object -->

            var regPackage = new RegistryPackageType()
            {
                id = extObjectID,
                objectType = "rn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:RegistryPackage"
            };
			var submissionTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            regPackage.Slot = new SlotType1[1] { getSlot("submissionTime", submissionTime) };

            regPackage.Classification = new ClassificationType[1]
            {
                new ClassificationType()
                {
                    classificationScheme = "urn:uuid:aa543740-bdda-424e-8c96-df4873be8500",
                    classifiedObject = extObjectID/*"urn:uuid:9029ab80-3681-4348-8550-aaacaa5a9c2a"*/,
                    id = "ContentTypeCode1",
                    nodeRepresentation = request.options.contentTypeCode.code,
                    objectType = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:Classification",
                    Slot = new SlotType1[1] { getSlot("codingScheme", 
						request.options.contentTypeCode.codingScheme) },
                    Name = getIStringType(request.options.contentTypeCode.display)
                }
               
            };
            regPackage.ExternalIdentifier = new ExternalIdentifierType[3] 
            {
                new ExternalIdentifierType() 
                {
                id = "id3",
                identificationScheme = "urn:uuid:6b5aea1a-874d-4603-a4bc-96a0a7b38446",
                objectType = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:ExternalIdentifier",
                registryObject = extObjectID,
				value = request.formattedIdentifier,
                Name = getIStringType("XDSSubmissionSet.patientId")
            },
                new ExternalIdentifierType() {
                id = "id4",
                identificationScheme = "urn:uuid:96fdda7c-d067-4183-912e-bf5ee74998a8" /* THIS NEEDS???? docID?? */,
                objectType = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:ExternalIdentifier",
                registryObject = extObjectID,
				value = XDSDocumentEntry_uniqueId,
                Name = getIStringType("XDSDocumentEntry.uniqueId")
            },
                 new ExternalIdentifierType(){
                id = "id5",
                identificationScheme = "urn:uuid:554ac39e-e3fe-47fe-b233-965d2a147832",
                objectType = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:ExternalIdentifier",
                registryObject = extObjectID/*"urn:uuid:9029ab80-3681-4348-8550-aaacaa5a9c2a"*/,
				/* this value should be AssigningAuthority - not sure */
                value = "1.3.6.1.4.1.21367.2010.1.2.300",
                Name = getIStringType("XDSSubmissionSet.sourceId")
            }
            };
            regObjList.Add(regPackage);
            // end RegistryPackage

            regObjList.Add(new ClassificationType()
            {
                classificationNode = "urn:uuid:a54d6aa5-d40d-43f9-88c5-b4633d873bdd",
                classifiedObject = extObjectID/*"urn:uuid:9029ab80-3681-4348-8550-aaacaa5a9c2a"*/,
                id = "isSS1",
                objectType = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:Classification"
            });

            regObjList.Add(new AssociationType1()
            {
                associationType = "urn:oasis:names:tc:ebxml-regrep:AssociationType:HasMember",
                id = "association1",
                objectType = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:Association",
                sourceObject = extObjectID/*"urn:uuid:9029ab80-3681-4348-8550-aaacaa5a9c2a"*/,
                targetObject = extObjectID/* "urn:uuid:acd0b09b-11bb-4141-b996-57925a4325d0"*/,
                Slot = new SlotType1[1] { getSlot("SubmissionSetStatus", "Original" ) }
            });
            xsdb_req.SubmitObjectsRequest.RegistryObjectList = regObjList.ToArray();
           
            
            /**/
         
            /**/
            return xsdb_req;
        }

		public XDSbClient() 
		{
		}
		public static string TestingEndpoint = 
            "http://exchange.healthshare.us:57772/csp/healthshare/hsrepository/HS.IHE.XDSb.Repository.Services.cls";

		public XDSbResponse ProvideAndRegisterDocument(XDSbRequest request)
		{
			return sendRequest( request );
		}

        private XDSbResponse sendRequest(XDSbRequest xdsb_request)
        {
			validate(xdsb_request);
			var url = this.ExchangeEndpoint;
            //var soap_action = "urn:ihe:iti:2007:RegisterDocumentSet-b";

            var soap_action = "urn:ihe:iti:2007:ProvideAndRegisterDocumentSet-b";
            HttpWebRequest request = PDQClient.CreateWebRequest(url, 
					soap_action, xdsb_request);
           
			/*
            */
			var query = getProvideAndRegisterRequest( xdsb_request );
            //var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ProvideAndRegisterDocumentSetRequestType), "urn:ihe:iti:xsd-b:2007");
            var extraTypes = new List<Type>();
            
            extraTypes.Add(typeof(ExtrinsicObjectType));
            extraTypes.Add(typeof(RegistryPackageType));
            extraTypes.Add(typeof(ClassificationType));
            extraTypes.Add(typeof(AssociationType1));
            extraTypes.Add(typeof(ProvideAndRegisterDocumentSetRequestTypeDocument));
            var serializer = new System.Xml.Serialization.XmlSerializer(
                typeof(ProvideAndRegisterDocumentSetRequestType), extraTypes.ToArray() );
            //var serializer = new System.Xml.Serialization.XmlSerializer(
            //    typeof(SubmitObjectsRequest), extraTypes.ToArray());

            var querySB = new StringBuilder();
            var xmlWriter = System.Xml.XmlWriter.Create(querySB, new XmlWriterSettings() { OmitXmlDeclaration = true });
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("xdsb", "urn:ihe:iti:xds-b:2007");
            //ns.Add("xdsb", "urn:ihe:iti:xds-b:2007");
            ns.Add("lcm", "urn:oasis:names:tc:ebxml-regrep:xsd:lcm:3.0");
            //ns.Add("FOOBAR", "urn:oasis:names:tc:ebxml-regrep:xsd:lcm:3.0");
            ns.Add("rim", "urn:oasis:names:tc:ebxml-regrep:xsd:rim:3.0");

            serializer.Serialize(xmlWriter, query, ns);
            var queryXML = querySB.ToString();
            trace(queryXML);
            XmlDocument soapEnvelopeXml = new XmlDocument();
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">";
            xml += @"  <soap:Header>
                <Security xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd""><UsernameToken>
                    <Username>HS_Services</Username>
                    <Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"">HS_Services</Password>
                    </UsernameToken></Security>  
                </soap:Header>";

            xml += "     <soap:Body>";
            xml += queryXML;
            xml += @"</soap:Body>
                </soap:Envelope>";
			trace(xml);
            soapEnvelopeXml.LoadXml(xml);

            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
			XDSbResponse xdsb_response;
            using (WebResponse response = request.GetResponse())
            {
				xdsb_response = XDSbResponse.CreateFromStream(
						response.GetResponseStream());
				/*
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    Console.WriteLine(soapResult);
                    Console.ReadLine();
                }
				*/
            }
			trace( "XDSbClient response="+xdsb_response.ToString() );
			if ( xdsb_response == null ) {
				Trace.TraceError("xdsb_response was null!");
			}
			return xdsb_response;
        }
    }
}
