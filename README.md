# MarketPulse Lab

MarketPulse Lab is a long-term financial market intelligence platform and
distributed-systems laboratory.

The platform will collect, normalize, distribute, persist and analyze financial
market data, initially focusing on cryptocurrency markets and simulated
cross-exchange arbitrage.

## Project goals

- Learn and operate distributed backend architectures.
- Use Kafka as the initial event backbone.
- Build independently deployable .NET services.
- Practice observability, resilience, CI/CD and infrastructure operations.
- Simulate failures and document system behavior.
- Evolve the platform through measurable architectural experiments.

## Initial flow

```text
Market Simulator
      |
      v
     Kafka
      |
      v
Market Storage
      |
      v
 PostgreSQL