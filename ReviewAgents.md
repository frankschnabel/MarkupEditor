# ReviewAgents: Code review requirements

Standards for reviewing **i-doxs** stand-alone applications. Use this file together with **`Agents.md`** (application behavior and architecture).

---

## Code Review Requirements

### General notes

- In the list of review issues, always include the file name and line number.
- Review results go into a file called ReviewResults.md

### Self-contained operation

- The application must run **without interactive prompts**; the only operational inputs are **command-line switches** and **configuration** (e.g., `App.config`).
- Files must be read from **toKubra** folder and written to **fromKubra** folder.

### Data & SQL

- All **`SELECT`** statements must include **`WITH (NOLOCK)`** to avoid table locking.
- **Parameterized queries only**—do not assemble executable SQL by concatenating **untrusted** or **runtime user** input.
- **`SqlBulkCopy`** and large batches: confirm **command timeouts**, **failure** behavior (partial load), and **transaction** scope where relevant.
- Review custom queries for potential performance issues.

### Security & compliance

- No **passwords**, full **connection strings**, or **KEIS credentials** in logs, comments, tests, or committed config.
- No **PII**, **PAN**, or **full bank account numbers** should be logged
- Treat generated **XML** and log output as **sensitive**; consider **path permissions**, **retention**, and **downstream** handling (PCI/PII).

### Reliability & operations

- New code paths should set **`ExitCodes`** consistently; **`Warning`** is defined but rarely/never used.
- **Multi-biller** runs: Failure for one biller **should not** prevent other billers running.
- **KEIS main vs. backup**: behavior should remain **visible in logs** (no silent connection failures).

### Performance & footprint

- Watch for **O(n²)** patterns (e.g. nested loops over large **`IN`** lists built from batch logins) and **per-batch memory**.
- **`BatchSize`** tuning vs. SQL **parameter count** and **memory** per batch.
- Avoid **excessive** memory or CPU use on large extracts (aligns with **Small footprint**).  If a large amount of data needs to be processed, either read it in batches or as a data stream (e.g., DataReader).

### Engineering hygiene

- **NuGet** / binding redirect changes: verify compatibility and test after upgrades (CsvHelper, Newtonsoft, Kubra packages).
- **NuGet packages and Compiled Code** (packages, obj, bin) should not be saved in GitHub.
- Non-trivial behavioral changes should update the **Agents.md** file and/or **ReadMe.md**.

### Code quality

- Avoid unused variables and methods.
- Avoid blocks of commented code.
- **Consistency**: Naming style (e.g., camelCase) of variables, methods, etc. should be consistent throughout the application.
- New **`[SuppressMessage]`** or static-analysis suppressions need a **short justification** tied to risk.
