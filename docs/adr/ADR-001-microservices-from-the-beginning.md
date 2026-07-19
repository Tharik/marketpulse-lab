# ADR-001: Adopt microservices from the beginning

- Status: Accepted
- Date: 2026-07-18

## Context

MarketPulse Lab is primarily a long-term technical learning environment.

Its goals include gaining practical experience with:

- distributed systems;
- asynchronous communication;
- independently deployable services;
- eventual consistency;
- observability;
- failure recovery;
- CI/CD;
- infrastructure operations.

The initial business volume does not require microservices. A modular monolith
would be operationally simpler.

However, architectural simplicity is not the only project objective. Creating a
realistic distributed environment is itself a required product outcome.

## Decision

MarketPulse Lab will begin with a small microservices architecture.

The initial system will contain only the services necessary to establish a real
distributed flow:

- Market Simulator;
- Market Storage.

Services must be independently executable, testable, deployable and observable.

New services must not be created solely to separate classes or technical layers.
Each service must own a clear business or platform responsibility.

## Alternatives considered

### Modular monolith

Advantages:

- lower operational complexity;
- simpler local development;
- easier transactions and debugging;
- faster initial delivery.

Disadvantages:

- delays practical distributed-systems learning;
- postpones Kafka integration and operational challenges;
- does not immediately exercise deployment and service-boundary decisions.

### Many fine-grained microservices

Advantages:

- exposes more distributed scenarios immediately.

Disadvantages:

- excessive cognitive and operational overhead;
- artificial service boundaries;
- increased risk of creating a distributed monolith.

## Consequences

Positive consequences:

- distributed-systems concerns become real from the first phase;
- Kafka, observability and deployment are exercised immediately;
- failures, retries, duplication and eventual consistency can be studied early.

Negative consequences:

- slower initial product delivery;
- more infrastructure;
- more difficult debugging;
- greater local resource consumption;
- more complex tests and deployments.

## Review criteria

This decision should be reviewed if:

- the environment becomes impractical on the available hardware;
- service boundaries produce excessive coupling;
- operational work prevents meaningful product progress;
- the architecture stops creating useful learning outcomes.