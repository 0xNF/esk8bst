using System;
using System.Collections.Generic;

namespace esk8lambda.models {

    interface IEmail {
        List<string> To { get; set; }
        List<string> CC { get; set; }
        List<string> BCC { get; set; }
        string From { get; set; }
        string Subject { get; set; }
        string Body { get; set; }
        byte[] Attachment { get; set; }

    }
    /// <summary>
    /// Email class that gets submitted to MailGun
    /// </summary>
    class MailgunEmail : IEmail {

        public List<string> To { get; set; } = new List<string>();
        public List<string> CC { get; set; } = new List<string>();
        public List<string> BCC { get; set; } = new List<string>();
        public string From { get; set; }
        public string Subject { get; set; }
        string BodyText { get; set; }
        string BodyHTML { get; set; }
        public string Body {
            get {
                if(String.IsNullOrWhiteSpace(BodyText)) {
                    return BodyHTML;
                }
                else {
                    return BodyText;
                }
            }
            set {; } // set is a noop on body
        }
        public byte[] Attachment { get; set; }
    }
}
