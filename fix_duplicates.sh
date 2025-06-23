#!/bin/bash

# This script removes the duplicate methods in BankPortal/Program.cs
# Usage: Run from the root of the SupplyChainFinance project

# Create a temporary file
temp_file=$(mktemp)

# Process the file to remove duplicate methods
awk '
BEGIN { skip = 0; }
/void AddFacilityToExistingLimit\(Organization customer, CreditLimitInfo creditLimit\)/ { 
  if (seen_add_facility) { 
    skip = 1;
    print "        // Duplicate method removed";
  } else {
    seen_add_facility = 1;
  }
}
/void CreateNewCreditLimitForCustomer\(Organization customer\)/ {
  if (seen_create_limit) {
    skip = 1;
    print "        // Duplicate method removed";
  } else {
    seen_create_limit = 1;
  }
}
/void ProcessFundingForInvoice\(Invoice invoice\)/ {
  if (seen_process_funding) { 
    skip = 1;
    print "        // Duplicate method removed";
  } else {
    seen_process_funding = 1;
  }
}
/^        }$/ {
  if (skip) {
    skip = 0;
    next;
  }
}
{ if (!skip) print; }
' BankPortal/Program.cs > "$temp_file"

# Replace the original file with the fixed version
mv "$temp_file" BankPortal/Program.cs

echo "Duplicate methods removed from BankPortal/Program.cs"
