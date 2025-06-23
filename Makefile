# Makefile for managing SupplyChainFinance application
.PHONY: clientportal bankportal clean setup shell down explore-db bank client

# Container name from docker-compose.yml
CONTAINER_NAME = supplychain-dev

# Core setup function to ensure Docker is ready
setup:
	@echo "Setting up development environment..."
	docker compose up -d
	docker exec $(CONTAINER_NAME) dotnet restore SupplyChainFinance.sln
	@echo "Development environment is ready!"

# Clean build artifacts to ensure fresh build
clean:
	@echo "Cleaning up build artifacts..."
	docker compose up -d
	docker exec $(CONTAINER_NAME) bash -c "find . -name bin -type d -exec rm -rf {} + 2>/dev/null || true"
	docker exec $(CONTAINER_NAME) bash -c "find . -name obj -type d -exec rm -rf {} + 2>/dev/null || true"
	@echo "Build artifacts cleaned!"

# Fix BankPortal methods to address known issues
fix-bankportal:
	@echo "Fixing method declarations in BankPortal..."
	docker compose up -d
	# Check if the fix scripts exist and make them executable
	docker exec $(CONTAINER_NAME) bash -c "if [ -f patch_methods.sh ]; then chmod +x patch_methods.sh && ./patch_methods.sh; fi"
	docker exec $(CONTAINER_NAME) bash -c "if [ -f fix_duplicates.sh ]; then chmod +x fix_duplicates.sh && ./fix_duplicates.sh; fi"
	docker exec $(CONTAINER_NAME) bash -c "if [ -f fix_additional_issues.sh ]; then chmod +x fix_additional_issues.sh && ./fix_additional_issues.sh; fi"
	@echo "BankPortal fixes applied!"

# Main commands - just these two for running the applications
clientportal: setup clean
	@echo "═══════════════════════════════════════════"
	@echo "       RUNNING CLIENT PORTAL"
	@echo "═══════════════════════════════════════════"
	docker compose up -d
	docker exec $(CONTAINER_NAME) dotnet build ClientPortal/ClientPortal.csproj
	docker exec -it $(CONTAINER_NAME) dotnet run --project ClientPortal/ClientPortal.csproj
	@echo "═══════════════════════════════════════════"
	@echo "    CLIENT PORTAL EXECUTION FINISHED"
	@echo "═══════════════════════════════════════════"

bankportal: setup clean fix-bankportal
	@echo "═══════════════════════════════════════════"
	@echo "       RUNNING BANK PORTAL"
	@echo "═══════════════════════════════════════════"
	docker compose up -d
	docker exec $(CONTAINER_NAME) dotnet build BankPortal/BankPortal.csproj
	docker exec -it $(CONTAINER_NAME) dotnet run --project BankPortal/BankPortal.csproj
	@echo "═══════════════════════════════════════════"
	@echo "    BANK PORTAL EXECUTION FINISHED"
	@echo "═══════════════════════════════════════════"

# Utilities
shell:
	@echo "Starting interactive shell in Docker container..."
	docker compose up -d
	docker exec -it $(CONTAINER_NAME) bash
	@echo "Shell session ended."

down:
	@echo "Stopping development environment..."
	docker compose down
	@echo "Development environment stopped!"

explore-db:
	@echo "Exploring SQLite database..."
	docker compose up -d
	docker exec -it $(CONTAINER_NAME) bash -c "apt-get update && apt-get install -y sqlite3 && sqlite3 supply_chain_finance.db"
	@echo "SQLite exploration complete!"

# Simple aliases for main commands
bank: bankportal
	@echo "Bank portal command completed!"

client: clientportal
	@echo "Client portal command completed!"