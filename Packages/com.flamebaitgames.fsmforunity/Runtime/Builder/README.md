# Runtime/Builder
Contains the logic for constructing a state machine. Typically, one would call FSMMachine.Build() to start construction. This returns an FSMMachine.IBuilder. This folder contains implementation details for that interface, along with extension methods for ease of use when building state machines.

By running the construction process via an interface, we are free to modify the details of its implementation in future releases. It also provides a simple type for users to define their own extension methods, and by doing so customizing their own workflow when implementing state machine construction.
