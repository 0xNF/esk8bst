using System;

namespace esk8lambda.models {
    [Flags]
    enum TransactionStatus {
        NONE = 2 ^ 0,
        OPEN = 2 ^ 1,
        CLOSED = 2 ^ 2
    }

}
