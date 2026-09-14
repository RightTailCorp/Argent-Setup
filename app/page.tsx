"use client";

import { useEffect, useMemo, useState } from "react";
import { ArgentBrand } from "./components/ArgentBrand";
import styles from "./wizard.module.css";

/**
 * Original legacy Setup: 9 screens.
 * Redesign: short flow + system check — if a required component is already on the PC, skip it;
 * if missing, Setup installs it from the bundled package (no secondary download).
 */
const STEPS = [
  "Start",
  "System check",
  "Install",
  "Paths & license",
  "Account",
  "Installing",
  "Done",
] as const;

type OpIndex = 0 | 1 | 2 | 3;
type CheckStatus = "pending" | "scanning" | "found" | "will_install";

type Prerequisite = {
  id: string;
  name: string;
  detail: string;
  status: CheckStatus;
};

const PREREQ_SEED: Prerequisite[] = [
  {
    id: "dotnet",
    name: ".NET Framework 4.8",
    detail: "Required runtime for Setup and client tools",
    status: "pending",
  },
  {
    id: "vcredist",
    name: "Visual C++ Redistributable",
    detail: "Native libraries used by Queue Engine / services",
    status: "pending",
  },
  {
    id: "odbc",
    name: "SQL Server ODBC Driver",
    detail: "Needed when SQL Server is selected as storage",
    status: "pending",
  },
  {
    id: "admin",
    name: "Administrator rights",
    detail: "Required to install Windows services and write Program Files",
    status: "pending",
  },
  {
    id: "disk",
    name: "Disk space (≈ 500 MB free)",
    detail: "On the drive you choose for install folders",
    status: "pending",
  },
];

/** Simulated scan results — web prototype (real Setup would query the machine). */
const SCAN_RESULTS: Record<string, CheckStatus> = {
  dotnet: "found", // already on this PC (matches your 4.8 setup log)
  vcredist: "will_install", // missing → install from Setup package
  odbc: "will_install",
  admin: "found",
  disk: "found",
};

export default function HomePage() {
  const [step, setStep] = useState(0);
  const [validation, setValidation] = useState("");
  const [licenseAccepted, setLicenseAccepted] = useState(false);
  const [installScheduler, setInstallScheduler] = useState(true);
  const [installQueue, setInstallQueue] = useState(true);
  const [opIndex, setOpIndex] = useState<OpIndex>(0);
  const [licensePath, setLicensePath] = useState(
    "D:\\ARGENT_JOB_SCHEDULER_10_0A_2401_A\\ARGENT_INSTALL_JOB_SCHEDULER_10_0_2401_64W_A"
  );
  const [standaloneQE, setStandaloneQE] = useState(true);
  const [qeKey, setQeKey] = useState("NC02-CI61-HE28-OL51-2DDF");
  const [schedulerPath, setSchedulerPath] = useState("C:\\ARGENT\\SchedulingEngine");
  const [queuePath, setQueuePath] = useState("C:\\ARGENT\\QueueEngine");
  const [useGmsa, setUseGmsa] = useState(false);
  const [account, setAccount] = useState("DESKTOP-IT4EK29\\layib");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [useSql, setUseSql] = useState(true);
  const [odbcDsn, setOdbcDsn] = useState("");
  const [showOdbcDialog, setShowOdbcDialog] = useState(false);
  const [showMoreContact, setShowMoreContact] = useState(false);
  const [email, setEmail] = useState("");
  const [contact, setContact] = useState("");
  const [company, setCompany] = useState("");
  const [address, setAddress] = useState("");
  const [city, setCity] = useState("");
  const [stateProv, setStateProv] = useState("");
  const [zip, setZip] = useState("");
  const [country, setCountry] = useState("");
  const [phone, setPhone] = useState("");
  const [salesRep, setSalesRep] = useState("");
  const [progress, setProgress] = useState(0);
  const [progressStatus, setProgressStatus] = useState("Preparing…");
  const [installNode, setInstallNode] = useState("DESKTOP-IT4EK29");
  const [prereqs, setPrereqs] = useState<Prerequisite[]>(PREREQ_SEED);
  const [scanDone, setScanDone] = useState(false);
  const [scanning, setScanning] = useState(false);

  const machine = "DESKTOP-IT4EK29";
  const user = "layib";
  const INSTALLING = 5;
  const DONE = 6;
  const pct = Math.round(
    ((step + (step === INSTALLING ? progress / 100 : 0)) / (STEPS.length - 1)) * 100
  );

  const missingCount = prereqs.filter((p) => p.status === "will_install").length;
  const foundCount = prereqs.filter((p) => p.status === "found").length;

  useEffect(() => {
    if (step !== 1) return;
    if (scanDone || scanning) return;

    setScanning(true);
    setPrereqs(PREREQ_SEED.map((p) => ({ ...p, status: "pending" })));

    let i = 0;
    const run = () => {
      if (i >= PREREQ_SEED.length) {
        setScanning(false);
        setScanDone(true);
        return;
      }
      const id = PREREQ_SEED[i].id;
      setPrereqs((list) =>
        list.map((p) => (p.id === id ? { ...p, status: "scanning" } : p))
      );
      window.setTimeout(() => {
        setPrereqs((list) =>
          list.map((p) =>
            p.id === id ? { ...p, status: SCAN_RESULTS[id] ?? "found" } : p
          )
        );
        i += 1;
        window.setTimeout(run, 180);
      }, 420);
    };
    run();
  }, [step, scanDone, scanning]);

  useEffect(() => {
    if (step !== INSTALLING) return;
    setProgress(0);
    const toInstall = prereqs.filter((p) => p.status === "will_install");
    setProgressStatus(
      toInstall.length
        ? `Installing bundled: ${toInstall[0].name}…`
        : "All prerequisites already present — copying Argent files…"
    );
    const id = window.setInterval(() => {
      setProgress((p) => {
        const next = p >= 90 ? Math.min(100, p + 1) : p + 2;
        if (toInstall.length && next < 35) {
          setProgressStatus(`Installing bundled: ${toInstall[0].name}…`);
        } else if (toInstall.length > 1 && next < 55) {
          setProgressStatus(`Installing bundled: ${toInstall[1].name}…`);
        } else if (next < 75) {
          setProgressStatus("Copying Argent program files…");
        } else if (next < 90) {
          setProgressStatus("Creating Queue Engine items…");
        } else {
          setProgressStatus("Finishing registry updates…");
        }
        if (next >= 100) {
          window.clearInterval(id);
          setStep(DONE);
        }
        return next;
      });
    }, 120);
    return () => window.clearInterval(id);
  }, [step, prereqs]);

  const canBack = step > 0 && step !== INSTALLING && step !== DONE;
  const canCancel = step !== INSTALLING && step !== DONE;
  const nextLabel =
    step === DONE
      ? "Close"
      : step === 0
        ? "Get started"
        : step === 1
          ? "Looks good — continue"
          : step === 4
            ? "Install"
            : "Continue";
  const nextDisabled = step === INSTALLING || (step === 1 && (!scanDone || scanning));

  const validate = (): boolean => {
    setValidation("");
    if (step === 0 && !licenseAccepted) {
      setValidation("Accept the license to continue.");
      return false;
    }
    if (step === 1 && !scanDone) {
      setValidation("Wait for the system check to finish.");
      return false;
    }
    if (step === 2 && !installScheduler && !installQueue) {
      setValidation("Pick at least one product.");
      return false;
    }
    if (step === 4) {
      if (password && password !== confirmPassword) {
        setValidation("Passwords do not match.");
        return false;
      }
      if (!email.trim()) {
        setValidation("Email is required.");
        return false;
      }
      if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email.trim())) {
        setValidation("Enter a valid email.");
        return false;
      }
    }
    return true;
  };

  const goNext = () => {
    if (step === DONE) {
      setStep(0);
      setProgress(0);
      setValidation("");
      setScanDone(false);
      setScanning(false);
      setPrereqs(PREREQ_SEED);
      return;
    }
    if (!validate()) return;

    if (step === 4 && useSql && !odbcDsn) {
      setShowOdbcDialog(true);
      return;
    }

    if (step === 4) {
      setStep(INSTALLING);
      return;
    }
    setStep((s) => Math.min(DONE, s + 1));
  };

  useEffect(() => {
    document.getElementById("wizard-content")?.scrollTo({ top: 0, behavior: "smooth" });
  }, [step]);

  const onOdbcNo = () => {
    setShowOdbcDialog(false);
    setUseSql(false);
    setOdbcDsn("");
    setValidation("SQL Server option turned off. You stay in Setup — nothing was cancelled.");
  };

  const onOdbcYes = () => {
    setShowOdbcDialog(false);
    setOdbcDsn("ArgentScheduler_DSN");
    setValidation("");
    setStep(INSTALLING);
  };

  const statusLabel = (s: CheckStatus) => {
    switch (s) {
      case "pending":
        return "Waiting";
      case "scanning":
        return "Checking…";
      case "found":
        return "Already on this PC";
      case "will_install":
        return "Missing — Setup will install";
    }
  };

  const content = useMemo(() => {
    switch (step) {
      case 0:
        return (
          <>
            <h1 className={styles.title}>Install Argent the easy way</h1>
            <p className={styles.lead}>
              Setup checks your PC first. If something required is already there, we skip it. If
              not, we install it from this package — no separate .NET downloads.
            </p>
            <div className={styles.promise}>
              <div>
                <strong>Self-contained Setup.</strong> Prerequisites ship inside the installer. You
                never chase “.Net 4.x.y.z…” from the web.
              </div>
            </div>
            <p className={styles.copy}>Close other apps if you can, then accept the license.</p>
            <textarea
              className={styles.licenseBox}
              readOnly
              value={`Software License and Usage Agreement -- Rev 001/Sep 2023

IMPORTANT - READ CAREFULLY

By exercising your rights to make and use copies of the Software, or keeping a copy or download for over 30 days, you agree to this Agreement.

This product is protected by U.S. Patents including 6483813; 511167; 511346; 530335; 543551; and related patents.

[Full license text unchanged from production installer.]`}
            />
            <label className={`${styles.choice} ${licenseAccepted ? styles.choiceActive : ""}`}>
              <input
                type="radio"
                name="license"
                checked={licenseAccepted}
                onChange={() => setLicenseAccepted(true)}
              />
              I accept the Agreement
            </label>
            <label className={`${styles.choice} ${!licenseAccepted ? styles.choiceActive : ""}`}>
              <input
                type="radio"
                name="license"
                checked={!licenseAccepted}
                onChange={() => setLicenseAccepted(false)}
              />
              I don&apos;t accept
            </label>
            <button type="button" className={styles.welcomeCta} onClick={goNext}>
              Get started
            </button>
          </>
        );
      case 1:
        return (
          <>
            <h1 className={styles.title}>System check</h1>
            <p className={styles.lead}>
              Scanning this PC for what Setup needs. Found items are left alone. Missing items are
              installed from the Setup package — still no web download.
            </p>
            <ul className={styles.checkList}>
              {prereqs.map((p) => (
                <li
                  key={p.id}
                  className={`${styles.checkRow} ${
                    p.status === "found"
                      ? styles.checkFound
                      : p.status === "will_install"
                        ? styles.checkInstall
                        : p.status === "scanning"
                          ? styles.checkScanning
                          : ""
                  }`}
                >
                  <div className={styles.checkIcon} aria-hidden>
                    {p.status === "found"
                      ? "✓"
                      : p.status === "will_install"
                        ? "+"
                        : p.status === "scanning"
                          ? "…"
                          : "○"}
                  </div>
                  <div className={styles.checkBody}>
                    <div className={styles.checkName}>{p.name}</div>
                    <div className={styles.checkDetail}>{p.detail}</div>
                  </div>
                  <div className={styles.checkBadge}>{statusLabel(p.status)}</div>
                </li>
              ))}
            </ul>
            {scanDone && (
              <div className={styles.promise}>
                <div>
                  <strong>
                    {foundCount} already OK
                    {missingCount ? ` · ${missingCount} will install from Setup` : ""}.
                  </strong>{" "}
                  Continue when you&apos;re ready — nothing leaves this machine for a second
                  download.
                </div>
              </div>
            )}
            {!scanDone && (
              <p className={styles.muted}>Checking your system… this takes a few seconds.</p>
            )}
          </>
        );
      case 2:
        return (
          <>
            <h1 className={styles.title}>What to install</h1>
            <p className={styles.lead}>Node, operation, and products.</p>
            <div className={styles.infoGrid}>
              <div className={styles.infoRow}>
                <span>Account</span>
                <strong>{user}</strong>
              </div>
              <div className={styles.infoRow}>
                <span>Domain / node</span>
                <strong>{machine}</strong>
              </div>
              <div className={styles.infoRow}>
                <span>Queue Engine</span>
                <strong>Not available</strong>
              </div>
              <div className={styles.infoRow}>
                <span>Job Scheduler</span>
                <strong>Not available</strong>
              </div>
            </div>
            <label className={styles.label}>Install on node</label>
            <input
              className={styles.input}
              value={installNode}
              onChange={(e) => setInstallNode(e.target.value)}
            />
            <div className={styles.sectionLabel}>Operation</div>
            {(
              [
                "Install Windows services + client programs",
                "Install client programs only",
                "Upgrade services and clients",
                "Deinstall",
              ] as const
            ).map((label, i) => (
              <label
                key={label}
                className={`${styles.choice} ${opIndex === i ? styles.choiceActive : ""}`}
              >
                <input
                  type="radio"
                  name="op"
                  checked={opIndex === i}
                  onChange={() => setOpIndex(i as OpIndex)}
                />
                {label}
              </label>
            ))}
            <div className={styles.sectionLabel} style={{ marginTop: 16 }}>
              Products
            </div>
            <label className={styles.check}>
              <input
                type="checkbox"
                checked={installScheduler}
                onChange={(e) => setInstallScheduler(e.target.checked)}
              />
              Argent Job Scheduler
            </label>
            <label className={styles.check}>
              <input
                type="checkbox"
                checked={installQueue}
                onChange={(e) => setInstallQueue(e.target.checked)}
              />
              Argent Queue Engine
            </label>
          </>
        );
      case 3:
        return (
          <>
            <h1 className={styles.title}>Paths &amp; license</h1>
            <p className={styles.lead}>
              License files and install folders — defaults match the current installer.
            </p>
            <label className={styles.label}>Job Scheduler license file</label>
            <div className={styles.row}>
              <input
                className={styles.input}
                value={licensePath}
                onChange={(e) => setLicensePath(e.target.value)}
              />
              <button type="button" className={styles.secondaryBtn}>
                Browse
              </button>
            </div>
            <label className={styles.check}>
              <input
                type="checkbox"
                checked={standaloneQE}
                onChange={(e) => setStandaloneQE(e.target.checked)}
              />
              Standalone Queue Engine
            </label>
            <label className={styles.label}>Queue Engine license key</label>
            <input
              className={styles.input}
              value={qeKey}
              onChange={(e) => setQeKey(e.target.value)}
            />
            <div className={styles.sectionLabel}>Install folders</div>
            <label className={styles.label}>Source (input)</label>
            <input
              className={styles.input}
              value="D:\\ARGENT_JOB_SCHEDULER_10_0A_2401_A\\_ARGENT_INSTALL_JOB_SCHEDULER_10_0_2401_64W_A"
              readOnly
            />
            <label className={styles.label}>Job Scheduler</label>
            <input
              className={styles.input}
              value={schedulerPath}
              onChange={(e) => setSchedulerPath(e.target.value)}
            />
            <label className={styles.label}>Queue Engine</label>
            <input
              className={styles.input}
              value={queuePath}
              onChange={(e) => setQueuePath(e.target.value)}
            />
          </>
        );
      case 4:
        return (
          <>
            <h1 className={styles.title}>Account &amp; contact</h1>
            <p className={styles.lead}>Service logon, SQL, and registration — then Install runs.</p>
            <label className={styles.check}>
              <input
                type="checkbox"
                checked={useGmsa}
                onChange={(e) => setUseGmsa(e.target.checked)}
              />
              Use Managed Service Account (gMSA)
            </label>
            <label className={styles.label}>Account (Domain\User)</label>
            <input
              className={styles.input}
              value={account}
              onChange={(e) => setAccount(e.target.value)}
            />
            <div className={styles.fieldGrid}>
              <div>
                <label className={styles.label}>Password</label>
                <input
                  className={styles.input}
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                />
              </div>
              <div>
                <label className={styles.label}>Confirm</label>
                <input
                  className={styles.input}
                  type="password"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                />
              </div>
            </div>
            <div className={styles.promise}>
              <div>
                SQL Server is the default (not CodeBase). CodeBase is fine for a small eval — not
                for production.
              </div>
            </div>
            <label className={styles.check}>
              <input
                type="checkbox"
                checked={useSql}
                onChange={(e) => setUseSql(e.target.checked)}
              />
              Use SQL Server (7.0+) as database storage
            </label>
            <div className={styles.row}>
              <span className={styles.muted}>
                {odbcDsn ? `DSN: ${odbcDsn}` : "No ODBC DSN yet"}
              </span>
              <button type="button" className={styles.secondaryBtn}>
                Advanced
              </button>
            </div>
            <div className={styles.sectionLabel}>Registration</div>
            <label className={styles.label}>Email</label>
            <input
              className={styles.input}
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="required"
            />
            <div className={styles.fieldGrid}>
              <div>
                <label className={styles.label}>Contact</label>
                <input
                  className={styles.input}
                  value={contact}
                  onChange={(e) => setContact(e.target.value)}
                />
              </div>
              <div>
                <label className={styles.label}>Company</label>
                <input
                  className={styles.input}
                  value={company}
                  onChange={(e) => setCompany(e.target.value)}
                />
              </div>
            </div>
            <button
              type="button"
              className={styles.ghostBtn}
              style={{ padding: "6px 0", minWidth: 0 }}
              onClick={() => setShowMoreContact((v) => !v)}
            >
              {showMoreContact ? "Hide address fields" : "More address fields (optional)"}
            </button>
            {showMoreContact && (
              <div className={styles.fieldGrid} style={{ marginTop: 8 }}>
                <div className={styles.fieldFull}>
                  <label className={styles.label}>Address</label>
                  <input
                    className={styles.input}
                    value={address}
                    onChange={(e) => setAddress(e.target.value)}
                  />
                </div>
                <div>
                  <label className={styles.label}>Town / City</label>
                  <input
                    className={styles.input}
                    value={city}
                    onChange={(e) => setCity(e.target.value)}
                  />
                </div>
                <div>
                  <label className={styles.label}>State / Province</label>
                  <input
                    className={styles.input}
                    value={stateProv}
                    onChange={(e) => setStateProv(e.target.value)}
                  />
                </div>
                <div>
                  <label className={styles.label}>ZIP / Postcode</label>
                  <input
                    className={styles.input}
                    value={zip}
                    onChange={(e) => setZip(e.target.value)}
                  />
                </div>
                <div>
                  <label className={styles.label}>Country</label>
                  <input
                    className={styles.input}
                    value={country}
                    onChange={(e) => setCountry(e.target.value)}
                  />
                </div>
                <div>
                  <label className={styles.label}>Phone</label>
                  <input
                    className={styles.input}
                    value={phone}
                    onChange={(e) => setPhone(e.target.value)}
                  />
                </div>
                <div>
                  <label className={styles.label}>Sales rep</label>
                  <input
                    className={styles.input}
                    value={salesRep}
                    onChange={(e) => setSalesRep(e.target.value)}
                  />
                </div>
              </div>
            )}
          </>
        );
      case 5:
        return (
          <>
            <h1 className={styles.title}>Installing</h1>
            <p className={styles.lead}>{progressStatus}</p>
            <div className={styles.barTrack}>
              <div className={styles.barFill} style={{ width: `${progress}%` }} />
            </div>
            <p className={styles.muted}>{progress}%</p>
            {missingCount > 0 && (
              <p className={styles.copy} style={{ marginTop: 16 }}>
                Missing prerequisites are being laid down from the Setup package first, then Argent
                files.
              </p>
            )}
          </>
        );
      default:
        return (
          <>
            <h1 className={styles.title}>You&apos;re set</h1>
            <p className={styles.lead}>Installed in about 266 seconds.</p>
            <ul className={styles.doneList}>
              <li>
                Prerequisites: {foundCount} already present
                {missingCount ? `, ${missingCount} installed from Setup` : ""}.
              </li>
              <li>Sample jobs are ready in Job Scheduler — copy and edit as needed.</li>
              <li>
                Sample queues/cmd files created. Default Queue Engine account: {machine}\{user}
              </li>
              <li>Support: Support@Argent.com · help.Argent.com</li>
            </ul>
          </>
        );
    }
  }, [
    step,
    licenseAccepted,
    prereqs,
    scanDone,
    foundCount,
    missingCount,
    installNode,
    opIndex,
    installScheduler,
    installQueue,
    licensePath,
    standaloneQE,
    qeKey,
    schedulerPath,
    queuePath,
    useGmsa,
    account,
    password,
    confirmPassword,
    useSql,
    odbcDsn,
    showMoreContact,
    email,
    contact,
    company,
    address,
    city,
    stateProv,
    zip,
    country,
    phone,
    salesRep,
    progress,
    progressStatus,
    machine,
    user,
  ]);

  return (
    <div className={styles.page}>
      <div className={styles.shell}>
        <aside className={styles.sidebar}>
          <div className={styles.sidebarInner}>
            <div className={styles.sidebarBrand}>
              <ArgentBrand />
              <div className={styles.setupLabel}>Self-contained Setup</div>
              <p className={styles.productLines}>
                Job Scheduler 10.0-2401-64W-A
                <br />
                Queue Engine 10.0-2401-64W-A
              </p>
            </div>
            <div className={styles.sidebarScroll}>
              <ol className={styles.stepList}>
                {STEPS.map((label, i) => {
                  const current = i === step;
                  const done = i < step;
                  const cls = current
                    ? styles.stepCurrent
                    : done
                      ? styles.stepDone
                      : styles.stepTodo;
                  return (
                    <li key={label} className={`${styles.stepItem} ${cls}`}>
                      <span className={styles.stepNum}>{done && !current ? "✓" : i + 1}</span>
                      <span className={styles.stepLabel}>{label}</span>
                    </li>
                  );
                })}
              </ol>
            </div>
            <p className={styles.sidebarHelp}>
              Questions?{" "}
              <a href="https://help.argent.com" target="_blank" rel="noreferrer">
                help.Argent.com
              </a>
            </p>
          </div>
        </aside>

        <div className={styles.main}>
          <header className={styles.mainTop}>
            <div className={styles.mainTopRow}>
              <span className={styles.stepPill}>{STEPS[step]}</span>
              <span className={styles.progressMeta}>
                Step {Math.min(step + 1, STEPS.length)} of {STEPS.length} · {Math.min(pct, 100)}%
              </span>
            </div>
            <div className={styles.progressTrack}>
              <div
                className={styles.progressFill}
                style={{ width: `${Math.min(pct, 100)}%` }}
              />
            </div>
          </header>

          <div className={styles.body}>
            <div className={styles.content} id="wizard-content">
              <div className={styles.contentInner}>{content}</div>
            </div>
            <footer className={styles.footer}>
              <div className={styles.validation}>{validation}</div>
              <div className={styles.actions}>
                <button
                  type="button"
                  className={styles.ghostBtn}
                  disabled={!canCancel}
                  onClick={() => {
                    if (window.confirm("Quit Setup?")) setStep(0);
                  }}
                >
                  Cancel
                </button>
                <button
                  type="button"
                  className={styles.secondaryBtn}
                  disabled={!canBack}
                  onClick={() => {
                    setValidation("");
                    setStep((s) => Math.max(0, s - 1));
                  }}
                >
                  Back
                </button>
                <button
                  type="button"
                  className={styles.primaryBtn}
                  disabled={nextDisabled}
                  onClick={goNext}
                >
                  {nextLabel}
                </button>
              </div>
            </footer>
          </div>
        </div>
      </div>

      {showOdbcDialog && (
        <div className={styles.modalOverlay}>
          <div className={styles.modal} role="dialog" aria-modal="true">
            <h2 className={styles.modalTitle}>Couldn’t open that ODBC source</h2>
            <p>Try another DSN? If you say No, you stay here — Setup does not exit.</p>
            <div className={styles.modalActions}>
              <button type="button" className={styles.secondaryBtn} onClick={onOdbcNo}>
                No — turn off SQL
              </button>
              <button type="button" className={styles.primaryBtn} onClick={onOdbcYes}>
                Yes — pick another
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
