# Shoe Shop (backend)

This is the backend of our shoe shop application. It's consisted of the following microservices:

- CartAPI - crud operations for shopping cart
- CommentsRatingsAPI - crud operations for shoe comments and ratings 
- DeliveryAPI - crud operations for shoe delivery
- InventoryAPI - crud operations for managing shoe inventory
- LoggingAPI - API to connect and display logs from RabbitMQ
- StatsAPI - crud operations to connect and display stats from Render
- UserAPI - crud operations to manage users

## Technology

<p>
<img src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/dot-net/dot-net-original.svg" alt=".NET "width="75px" padding-right="5px" />
<img src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/react/react-original.svg" alt="React" width="75px" padding-right="5px" />         
<img src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/docker/docker-original.svg" alt="Docker" width="75px" padding-right="5px" />
<img src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/rabbitmq/rabbitmq-original.svg" alt="RabbitMQ" width="75px"/>
          
</p>

## Getting Started

Follow these steps to set up and run the project:

### 1. Install Visual Studio

Ensure you have Visual Studio installed on your system. The recommended edition is Visual Studio Community, Professional, or Enterprise. Download it from the [official Visual Studio website](https://visualstudio.microsoft.com/).

During installation, include the following workloads:

- ASP.NET and web development
- .NET desktop development

### 2. Clone the Repository
Pull the code from your repository to your local machine using the command line or a Git client:

`git clone https://github.com/zbrdarovski/shoe-shop-backend.git`

`cd shoe-shop-backend`  

### 3. Open the Project in Visual Studio

Launch Visual Studio.
Open the solution file (.sln) in the cloned repository by navigating to File > Open > Project/Solution.

### 4. Select Build Configuration

Visual Studio allows you to choose between two build configurations:

- Debug: Ideal for development and testing, with debugging features enabled
- Release: Optimized for production deployment

You can select the desired configuration from the Configuration Manager in the toolbar.

### 5. Build the Project

Click on Build > Build Solution or press Ctrl + Shift + B to compile the project.

### 6. Run the Project

Use the Debug button in Visual Studio to run the project in Debug mode. This will start the service and attach the debugger.
Verify the service is running by checking the console output or accessing the URL specified in the application.

Additional Notes

> Make sure required dependencies (e.g., a database or third-party services) are installed and configured if applicable.
> 
> You can also configure the microservice to run in Docker if supported by your project setup.

## License

This project is not licensed for public use. The code is protected by copyright law.  
Viewing is permitted for evaluation purposes only. Copying, modifying, or distributing the code is strictly prohibited.

Copyright (c) 2024 Zdravko Brdarovski and [Anton's GitHub](https://github.com/Tonskii)
