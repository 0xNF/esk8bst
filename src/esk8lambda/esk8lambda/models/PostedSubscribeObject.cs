using System.Collections.Generic;

namespace esk8lambda.models {
    class PostedSubscribeObject {
        public string Email { get; set; }
        public List<PostedMatchObject> Matches { get; set; }
    }

}
