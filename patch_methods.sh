#!/bin/bash
# This script fixes the method declarations in BankPortal/Program.cs

sed -i 's/private void WaitForEnterKey/void WaitForEnterKey/g' BankPortal/Program.cs
sed -i 's/private void ManageOrganization/void ManageOrganization/g' BankPortal/Program.cs
sed -i 's/private void ManageOrganizationFacilities/void ManageOrganizationFacilities/g' BankPortal/Program.cs
sed -i 's/private void CreateNewCreditLimit/void CreateNewCreditLimit/g' BankPortal/Program.cs
sed -i 's/private void ModifyExistingFacility/void ModifyExistingFacility/g' BankPortal/Program.cs
sed -i 's/private void ViewAllocatedFacilities/void ViewAllocatedFacilities/g' BankPortal/Program.cs
sed -i 's/private void AddFacilityToExistingLimit/void AddFacilityToExistingLimit/g' BankPortal/Program.cs
