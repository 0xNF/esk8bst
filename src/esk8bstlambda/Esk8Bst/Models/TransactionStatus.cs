using System;

namespace Esk8Bst.Models {
    [Flags]
    public enum TransactionStatus {
        NONE = 2 ^ 0,
        OPEN = 2 ^ 1,
        CLOSED = 2 ^ 2
    }


}
