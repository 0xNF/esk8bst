using System;

namespace Esk8Bst.Models {

    [Flags]
    public enum BST {
        NONE = 2 ^ 0,
        BUY = 2 ^ 1,
        SELL = 2 ^ 2,
        TRADE = 2 ^ 3,
        BST = 2 ^ 4,
    }

}
