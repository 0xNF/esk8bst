interface SortFilter {
    Field: string;
    Order: string;
    Company: string[];
    BST: string;
  }

  const DefaultSortFilter: SortFilter = {
    Field: "date_posted",
    Order: "down",
    Company: ["any"],
    BST: "bst"
  }

  export { SortFilter, DefaultSortFilter }