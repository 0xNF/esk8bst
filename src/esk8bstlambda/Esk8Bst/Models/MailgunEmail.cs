using System;
using System.Collections.Generic;

namespace Esk8Bst.Models {
    /// <summary>
    /// Email class that gets submitted to MailGun
    /// </summary>
    public class MailgunEmail : IEmail {

        public List<string> To { get; set; } = new List<string>();
        public List<string> CC { get; set; } = new List<string>();
        public List<string> BCC { get; set; } = new List<string>();
        public string From { get; set; }
        public string Subject { get; set; }
        string BodyText { get; set; }
        string BodyHTML { get; set; }
        public string Body {
            get {
                if (String.IsNullOrWhiteSpace(BodyText)) {
                    return BodyHTML;
                }
                else {
                    return BodyText;
                }
            }
            set {
                BodyText = value;
            } // set is a noop on body
        }
        public bool IsTest { get; set; } = false;
        public byte[] Attachment { get; set; }
    }


}
