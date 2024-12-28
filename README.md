# Lyvads Project (RADIKSEZ)

**Lyvads** is a dynamic platform connecting creators and users for seamless collaboration and personalized content delivery. This project focuses on providing a secure, scalable, and user-friendly experience for its diverse roles, including Creators, Regular Users, Admins, and SuperAdmins.

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Features](#features)
3. [Technologies Used](#technologies-used)
4. [Installation](#installation)
5. [Usage](#usage)
6. [API Documentation](#api-documentation)
7. [Contributing](#contributing)
8. [License](#license)
9. [Contact](#contact)

---

## Project Overview

The Lyvads platform solves the challenge of creating a secure and collaborative ecosystem for content creators and users. It allows:

- **Efficient content management** for creators.
- **Secure financial transactions**, including payments and withdrawals.
- **Seamless user collaboration** via in-app messaging.

The platform is designed to ensure security, scalability, and performance to meet the needs of an expanding user base.

---

## Features

- **Multi-Role System:** Secure access tailored for Creators, Regular Users, Admins, and SuperAdmins.
- **Payment Integration:** Integrated Paystack API and custom wallet system for seamless financial transactions.
- **Collaboration Tools:** In-app messaging and content request features for user-creator interaction.
- **Scalable APIs:** Optimized for high performance and reliability.
- **Role-Based Access Control:** Robust authentication and security mechanisms.

---

## Technologies Used

- **Backend Framework:** C# with .NET Core
- **Database:** Microsoft SQL Server
- **Payment Integration:** Paystack API
- **APIs:** RESTful services
- **Authentication:** Secure role-based access control
- **Collaboration Features:** In-app messaging

---

## Installation

### Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- Paystack API credentials

### Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/lyvads-project.git
   cd lyvads-project

2. Navigate to the project directory:

    ```bash
    cd lyvads-project
    ```

3. Install dependencies for the backend:

    ```bash
    cd lyvads-project
    dotnet restore
    ```

4. Configure environment variables:
   - Create a `.env` file in the backend directory and add your database URL, API keys, etc.

5. Start the backend server:

    ```bash
    dotnet run
    ```
6. Run database migrations to set up the database schema:

    ```bash
    dotnet ef database update
    ```
7. Open your browser and go to `http://localhost:5000` to access the backend.

## Usage

Once the system is up and running, users can:

- **Creators**: Upload and manage personalized content, communicate with users, and withdraw earnings securely.
- **Regular Users**: Request personalized content, engage with creators through in-app messaging, and manage subscriptions.
- **Admins**: Monitor platform activities, manage users, and generate reports for platform performance.
- **SuperAdmins**: Oversee platform-wide operations, manage roles, and ensure system integrity.

### Example Use Cases

1. **Content Upload**: Creators can upload new content and manage their portfolio directly on the platform.
   
2. **Content Requests**: Users can request personalized content from their favorite creators.

3. **Secure Transactions**: All financial transactions, including payments and withdrawals, are processed through the integrated Paystack system.
   
4. **Role-Based Management**: Admins and SuperAdmins manage platform users, assign roles, and oversee activities.

## API Documentation

For detailed API usage and endpoints, see the [API Documentation](API_DOC.md).

## Contributing

If you'd like to contribute to the development of the Lyvads Project, please follow these steps:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature/your-feature`).
3. Make your changes and commit (`git commit -am 'Add new feature'`).
4. Push to the branch (`git push origin feature/your-feature`).
5. Create a pull request explaining your changes.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact

For any inquiries or issues, feel free to reach out:

- **Email**: ogbeidemurphy@gmail.com
- **GitHub**: [https://github.com/murphite](https://github.com/murphite)
- **Project Repository**: [https://github.com/murphite/PRMS-BE](https://github.com/murphite/PRMS-BE)

