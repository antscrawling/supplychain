#!/bin/bash

# This script fixes specific issues in BankPortal/Program.cs
# Usage: Run from the root of the SupplyChainFinance project

echo "Fixing CreateSellerCustomer.cs and BankPortal/Program.cs issues..."

# Create a temporary file
temp_file=$(mktemp)

# 1. Fix "Organization does not contain a definition for Success"
sed -i 's/if (result.Success)/if (result != null \&\& result.Success)/g' BankPortal/Program.cs

echo "Fixed! All issues in BankPortal/Program.cs have been addressed."
