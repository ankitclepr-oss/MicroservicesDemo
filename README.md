Microservices Architecture with .NET, gRPC & RabbitMQ

This project demonstrates a modern microservices-based architecture built using .NET, showcasing communication patterns using gRPC and event-driven messaging with RabbitMQ (via MassTransit).

🚀 Overview

The system is composed of multiple independent services that communicate using a combination of:

gRPC (synchronous communication) for fast, contract-based service-to-service calls
RabbitMQ + MassTransit (asynchronous communication) for event-driven workflows

This hybrid approach reflects real-world distributed system design, balancing performance and scalability.

🧩 Key Components
Order Service
Acts as the entry point (API layer using controllers)
Publishes events (e.g., order created) to RabbitMQ
Payment Service
Handles payment processing
Communicates via gRPC for synchronous operations
Email Service
Consumes events from RabbitMQ
Sends notifications (simulated)
⚙️ Tech Stack
.NET (ASP.NET Core Web API)
gRPC
MassTransit
RabbitMQ
C#
🎯 Key Features
Clean separation of concerns using microservices
Event-driven architecture with message broker
gRPC-based inter-service communication
Scalable and loosely coupled design
Controller-based API (no minimal APIs)
