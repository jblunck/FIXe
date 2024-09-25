# How to run multiple fault-tolerant Acceptors in parallel

```
$ k get pods
acceptor-0               1/1       Running   0          5d
acceptor-1               1/1       Running   0          5d
$ k get svc
NAME      TYPE           CLUSTER-IP     EXTERNAL-IP   PORT(S)        AGE
acceptor  LoadBalancer   10.110.22.74   <pending>     80:32710/TCP   5d
```

1. TCP connection gets established to acceptor-0 (OR1:acceptor)
2. receives 35=D (NOS)
3. acceptor-0 publishes ClOrdID
4. acceptor-0 sends request to NPS
5. acceptor-0 publishes pending confirmation
6. TCP drops

1. acceptor-1 receives ClOrdID
2. acceptor-1 receives pending confirmation
3. TCP connection gets established to acceptor-1 (OR1:acceptor)

- at this point the state needs to be refreshed and the messagestore needs to be updated
- the internal state of the orders needs to be aligned with what is found in the messagestore
- Q: what happens if the second connection is slow or offline at that point?
- Q: what happens if the second connection is ahead at that point?

4. acceptor-1 receives Execution Report
5. acceptor-1 sends Execution Report to OR1



How does the instance of the statefulset find out how many other instances it needs to connect to?

acceptor-1 subscribe to acceptor-0 pub

acceptor-0 subscribe to acceptor-1 pub
