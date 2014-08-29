using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace InterSystems.IHE.Client 
{
    public class XDSbRequest : IHERequest
    {
		private List<string> requiredOptions;
        public XDSbRequest()
        {
            //string patientId, string idSource,MimeType mimeType, byte[] data, dynamic options)
            this.creationDateTime = DateTime.Now;
			this.requiredOptions = new List<String>() 
				{ "language", "classCode", "confidentialityCode", "formatCode",
				  "healthcareFacilityTypeCode", "practiceSettingCode",
				  "typeCode", "contentTypeCode" };

        }
        public static dynamic getDefaultOptions()
        {

            Func<string,string,object> lookup = (ct,code1) =>  XDSCodeSet.Codes(ct).FirstOrDefault(c => c.code.Equals(code1));
            return new
            {
                language = "EN",
                /*classCode = lookup("classCode","34133-9"),*/
				classCode = XDSCodeSet.Code("11488-4","2.16.840.1.113883.6.1","typeCode"),
                confidentialityCode = lookup("confidentialityCode","N"),
				formatCode = lookup("formatCode","ScanPDF/IHE 1.x"),
				healthcareFacilityTypeCode = lookup("healthcareFacilityTypeCode","COMM"),
				practiceSettingCode = lookup("practiceSettingCode","Multidisciplinary"),
				typeCode = lookup("typeCode","34133-9"), /* Summarization of Episode Note */
				contentTypeCode = lookup("contentTypeCode","Summarization of episode")

            };
        }
        public string patientId { get; set; }
        public string idSource { get; set; }
        public dynamic options { get; set; }
        public MimeType mimeType { get; set; }
        public DateTime creationDateTime { get; set; }
        public byte[] data { get; set; }
        public string formattedIdentifier
        {
            get
            {
                //100000001^^^&1.3.6.1.4.1.21367.2010.1.2.300&ISO
                return this.patientId + "^^^&" + this.idSource + "&ISO";
            }
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("patientId=" + this.patientId).Append(",");
            sb.Append("idSource=" + this.idSource).Append(",");
            sb.Append("mimeType=" + this.mimeType).Append(",");
            sb.Append("creationDateTime=" + this.creationDateTime).Append(",");
            sb.Append("options=" + this.options).Append(",");
            sb.Append("data.length=" + this.data.Length);
            return sb.ToString();
        }
        public bool Validate()
        {
			bool valid = true;
            if (this.patientId == null)
            {
                Trace.TraceError("XDSbRequest.Validate patientId was null");
                valid = false;
            }
            if (this.idSource == null)
            {
                Trace.TraceError("XDSbRequest.Validate isSource was null");
                valid = false;
            }
            if (this.mimeType == null)
            {
                Trace.TraceError("XDSbRequest.Validate mimeType was null");
                valid = false;
            }
            if (this.data == null)
            {
                Trace.TraceError("XDSbRequest.Validate data was null");
                valid = false;
            }
			if ( this.options == null ) 
			{
				Trace.TraceError("XDSbRequest.Validate options was null");
				valid = false;
			}
			if ( this.options != null ) {
				var type = this.options.GetType();
				foreach(var ro in this.requiredOptions) {
					if ( type.GetProperty(ro) == null ) {
						Trace.TraceError( "XDSbRequest.Validate " +
							"Missing required " + ro + " option "); 
						valid = false;
					} 
				}
			}
			if ( valid ) {
	            Trace.TraceInformation("XDSbRequest.Validate OK");
    		}
	        Trace.TraceInformation("XDSbRequest.Validate request=" + this.ToString());
            return valid;
        }
    }
}
