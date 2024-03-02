# Move generator benchmark overview


## 2nd March 2024 Summary

### Summary

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3155/23H2/2023Update/SunValley3)

Intel Core i7-8086K CPU 4.00GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores

.NET SDK 8.0.200

[Host]     : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2

DefaultJob : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2

### Stats

| Method                      | _fen                 | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|---------------------------- |--------------------- |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| GenerateMovesNoPool         | 8/2p5(...)- 0 1 [41] |   299.1 ns |  5.93 ns |  6.09 ns |  1.00 |    0.00 | 0.2866 | 0.0019 |    1800 B |        1.00 |
| GenerateMovesUnsafeNoPool   | 8/2p5(...)- 0 1 [41] |   294.9 ns |  5.17 ns |  9.96 ns |  1.00 |    0.05 | 0.2866 | 0.0019 |    1800 B |        1.00 |
| GenerateMovesWithPool       | 8/2p5(...)- 0 1 [41] |   218.4 ns |  1.59 ns |  1.33 ns |  0.73 |    0.01 |      - |      - |         - |        0.00 |
| GenerateMovesUnsafeWithPool | 8/2p5(...)- 0 1 [41] |   208.2 ns |  2.12 ns |  1.98 ns |  0.70 |    0.01 |      - |      - |         - |        0.00 |
| GenerateMovesEnumerated     | 8/2p5(...)- 0 1 [41] |   911.2 ns | 17.83 ns | 19.08 ns |  3.05 |    0.09 | 1.2341 | 0.0057 |    7744 B |        4.30 |
|                             |                      |            |          |          |       |         |        |        |           |             |
| GenerateMovesNoPool         | r3k2r(...)- 0 1 [68] |   460.7 ns |  8.48 ns |  7.52 ns |  1.00 |    0.00 | 0.2866 | 0.0019 |    1800 B |        1.00 |
| GenerateMovesUnsafeNoPool   | r3k2r(...)- 0 1 [68] |   409.2 ns |  8.01 ns |  7.49 ns |  0.89 |    0.01 | 0.2866 | 0.0019 |    1800 B |        1.00 |
| GenerateMovesWithPool       | r3k2r(...)- 0 1 [68] |   361.5 ns |  6.44 ns |  5.71 ns |  0.79 |    0.02 |      - |      - |         - |        0.00 |
| GenerateMovesUnsafeWithPool | r3k2r(...)- 0 1 [68] |   316.7 ns |  2.92 ns |  2.44 ns |  0.69 |    0.01 |      - |      - |         - |        0.00 |
| GenerateMovesEnumerated     | r3k2r(...)- 0 1 [68] | 1,635.0 ns | 29.18 ns | 27.29 ns |  3.54 |    0.07 | 1.3142 | 0.0095 |    8248 B |        4.58 |
|                             |                      |            |          |          |       |         |        |        |           |             |
| GenerateMovesNoPool         | r3k2r(...)- 0 1 [64] |   230.8 ns |  4.64 ns |  5.52 ns |  1.00 |    0.00 | 0.2868 | 0.0019 |    1800 B |        1.00 |
| GenerateMovesUnsafeNoPool   | r3k2r(...)- 0 1 [64] |   231.3 ns |  4.14 ns |  6.68 ns |  1.01 |    0.04 | 0.2866 | 0.0019 |    1800 B |        1.00 |
| GenerateMovesWithPool       | r3k2r(...)- 0 1 [64] |   146.8 ns |  1.26 ns |  1.05 ns |  0.63 |    0.02 |      - |      - |         - |        0.00 |
| GenerateMovesUnsafeWithPool | r3k2r(...)- 0 1 [64] |   146.3 ns |  1.57 ns |  1.31 ns |  0.63 |    0.02 |      - |      - |         - |        0.00 |
| GenerateMovesEnumerated     | r3k2r(...)- 0 1 [64] |   923.7 ns |  9.21 ns |  8.62 ns |  3.98 |    0.10 | 1.3180 | 0.0057 |    8272 B |        4.60 |
|                             |                      |            |          |          |       |         |        |        |           |             |
| GenerateMovesNoPool         | r3k2r(...)- 0 1 [50] |   357.3 ns |  4.38 ns |  3.66 ns |  1.00 |    0.00 | 0.2866 | 0.0019 |    1800 B |        1.00 |
| GenerateMovesUnsafeNoPool   | r3k2r(...)- 0 1 [50] |   357.1 ns |  6.86 ns |  7.90 ns |  1.00 |    0.03 | 0.2866 | 0.0019 |    1800 B |        1.00 |
| GenerateMovesWithPool       | r3k2r(...)- 0 1 [50] |   268.2 ns |  3.69 ns |  3.27 ns |  0.75 |    0.01 |      - |      - |         - |        0.00 |
| GenerateMovesUnsafeWithPool | r3k2r(...)- 0 1 [50] |   253.6 ns |  4.83 ns |  4.28 ns |  0.71 |    0.01 |      - |      - |         - |        0.00 |
| GenerateMovesEnumerated     | r3k2r(...)- 0 1 [50] | 1,065.2 ns | 20.78 ns | 34.72 ns |  3.01 |    0.15 | 1.2436 | 0.0076 |    7808 B |        4.34 |
|                             |                      |            |          |          |       |         |        |        |           |             |
| GenerateMovesNoPool         | r4rk1(...) 0 10 [72] |   556.1 ns |  4.96 ns |  4.40 ns |  1.00 |    0.00 | 0.2861 | 0.0019 |    1800 B |        1.00 |
| GenerateMovesUnsafeNoPool   | r4rk1(...) 0 10 [72] |   483.9 ns |  3.33 ns |  2.78 ns |  0.87 |    0.01 | 0.2861 | 0.0019 |    1800 B |        1.00 |
| GenerateMovesWithPool       | r4rk1(...) 0 10 [72] |   406.8 ns |  3.37 ns |  2.99 ns |  0.73 |    0.01 |      - |      - |         - |        0.00 |
| GenerateMovesUnsafeWithPool | r4rk1(...) 0 10 [72] |   387.9 ns |  3.80 ns |  2.97 ns |  0.70 |    0.01 |      - |      - |         - |        0.00 |
| GenerateMovesEnumerated     | r4rk1(...) 0 10 [72] | 1,755.4 ns | 34.98 ns | 53.42 ns |  3.12 |    0.07 | 1.3142 | 0.0095 |    8248 B |        4.58 |
|                             |                      |            |          |          |       |         |        |        |           |             |
| GenerateMovesNoPool         | rnbqk(...) 5 25 [55] |   278.8 ns |  1.77 ns |  1.57 ns |  1.00 |    0.00 | 0.2866 | 0.0019 |    1800 B |        1.00 |
| GenerateMovesUnsafeNoPool   | rnbqk(...) 5 25 [55] |   272.8 ns |  5.14 ns |  4.81 ns |  0.98 |    0.02 | 0.2866 | 0.0019 |    1800 B |        1.00 |
| GenerateMovesWithPool       | rnbqk(...) 5 25 [55] |   210.9 ns |  1.07 ns |  0.90 ns |  0.76 |    0.00 |      - |      - |         - |        0.00 |
| GenerateMovesUnsafeWithPool | rnbqk(...) 5 25 [55] |   196.9 ns |  1.41 ns |  1.18 ns |  0.71 |    0.01 |      - |      - |         - |        0.00 |
| GenerateMovesEnumerated     | rnbqk(...) 5 25 [55] | 1,077.0 ns | 14.41 ns | 12.77 ns |  3.86 |    0.06 | 1.2798 | 0.0076 |    8040 B |        4.47 |
|                             |                      |            |          |          |       |         |        |        |           |             |
| GenerateMovesNoPool         | rnbqk(...)- 0 1 [56] |   227.3 ns |  3.20 ns |  3.00 ns |  1.00 |    0.00 | 0.2868 | 0.0019 |    1800 B |        1.00 |
| GenerateMovesUnsafeNoPool   | rnbqk(...)- 0 1 [56] |   224.4 ns |  4.49 ns |  5.17 ns |  0.99 |    0.02 | 0.2868 | 0.0019 |    1800 B |        1.00 |
| GenerateMovesWithPool       | rnbqk(...)- 0 1 [56] |   151.5 ns |  0.75 ns |  0.63 ns |  0.67 |    0.01 |      - |      - |         - |        0.00 |
| GenerateMovesUnsafeWithPool | rnbqk(...)- 0 1 [56] |   151.8 ns |  2.89 ns |  3.55 ns |  0.67 |    0.02 |      - |      - |         - |        0.00 |
| GenerateMovesEnumerated     | rnbqk(...)- 0 1 [56] | 1,056.0 ns |  6.40 ns |  5.35 ns |  4.65 |    0.06 | 1.2951 | 0.0057 |    8128 B |        4.52 |
