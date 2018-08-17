using System.Collections.Generic;

namespace Esk8Bst.Models {

    public interface IEmail {
        List<string> To { get; set; }
        List<string> CC { get; set; }
        List<string> BCC { get; set; }
        string From { get; set; }
        string Subject { get; set; }
        string Body { get; set; }
        byte[] Attachment { get; set; }
    }


}
