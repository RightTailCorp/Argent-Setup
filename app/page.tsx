"use client";

import { useEffect, useMemo, useState } from "react";
import { ArgentBrand } from "./components/ArgentBrand";
import styles from "./wizard.module.css";

const STEPS = [
  "Welcome",
  "License",
  "This computer",
  "What to install",
  "License files",
  "Folders",
  "Service & SQL",
  "Your details",
  "Installing",
  "Done",
] as const;

type OpIndex = 0 | 1 | 2 | 3;

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

  const machine = "DESKTOP-IT4EK29";
  const user = "layib";
  const pct = Math.round(((step + (step === 8 ? progress / 100 : 0)) / (STEPS.length - 1)) * 100);

  useEffect(() => {
    if (step !== 8) return;
    setProgress(0);
    setProgressStatus("Updating system registry…");
    const id = window.setInterval(() => {
      setProgress((p) => {
        const next = p >= 90 ? Math.min(100, p + 1) : p + 2;
        if (next >= 50 && next < 85) setProgressStatus("Creating Queue Engine items…");
        else if (next >= 85) setProgressStatus("Finishing registry updates…");
        if (next >= 100) {
          window.clearInterval(id);
          setStep(9);
        }
        return next;
      });
    }, 120);
    return () => window.clearInterval(id);
  }, [step]);

  const canBack = step > 0 && step !== 8 && step !== 9;
  const canCancel = step !== 8 && step !== 9;
  const nextLabel = step === 9 ? "Close" : step === 0 ? "Get started" : "Continue";
  const nextDisabled = step === 8;

  const validate = (): boolean => {
    setValidation("");
    if (step === 1 && !licenseAccepted) {
      setValidation("Accept the license to continue.");
      return false;
    }
    if (step === 3 && !installScheduler && !installQueue) {
      setValidation("Pick at least one product.");
      return false;
    }
    if (step === 6 && password && password !== confirmPassword) {
      setValidation("Passwords do not match.");
      return false;
    }
    if (step === 7) {
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
    if (step === 9) {
      setStep(0);
      setProgress(0);
      setValidation("");
      return;
    }
    if (!validate()) return;

    if (step === 6 && useSql && !odbcDsn) {
      setShowOdbcDialog(true);
      return;
    }

    if (step === 7) {
      setStep(8);
      return;
    }
    setStep((s) => Math.min(9, s + 1));
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
    setStep(7);
  };

  const content = useMemo(() => {
    switch (step) {
      case 0:
        return (
          <>
            <h1 className={styles.title}>Install Argent in a few steps</h1>
            <p className={styles.lead}>
              Job Scheduler and Queue Engine — one Setup, one path. Built for busy admins who
              should not have to chase extra installers.
            </p>
            <div className={styles.promise}>
              <div>
                <strong>Everything is inside this Setup.</strong> No secondary downloads of .NET
                Framework (or “.Net 4.x.y.z…”). Runtime pieces ship embedded — you run Setup and
                go.
              </div>
            </div>
            <p className={styles.copy}>
              Close other apps if you can, then continue. You can go back at any time before
              install starts.
            </p>
            <p className={styles.warn}>
              Protected by copyright and international treaties. Unauthorized copying or
              distribution may bring civil and criminal penalties.
            </p>
            <button type="button" className={styles.welcomeCta} onClick={goNext}>
              Get started
            </button>
          </>
        );
      case 1:
        return (
          <>
            <h1 className={styles.title}>License</h1>
            <p className={styles.lead}>Quick read, then accept to continue.</p>
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
          </>
        );
      case 2:
        return (
          <>
            <h1 className={styles.title}>This computer</h1>
            <p className={styles.lead}>Confirm the node where Setup will run.</p>
            <div className={styles.infoGrid}>
              <div className={styles.infoRow}>
                <span>Account</span>
                <strong>{user}</strong>
              </div>
              <div className={styles.infoRow}>
                <span>Domain</span>
                <strong>{machine}</strong>
              </div>
              <div className={styles.infoRow}>
                <span>Node</span>
                <strong>{machine}</strong>
              </div>
            </div>
            <label className={styles.label}>Install on node</label>
            <input
              className={styles.input}
              value={installNode}
              onChange={(e) => setInstallNode(e.target.value)}
            />
            <div className={styles.sectionLabel}>Program status</div>
            <div className={styles.infoGrid}>
              <div className={styles.infoRow}>
                <span>Queue Engine</span>
                <strong>Not available</strong>
              </div>
              <div className={styles.infoRow}>
                <span>Job Scheduler</span>
                <strong>Not available</strong>
              </div>
            </div>
          </>
        );
      case 3:
        return (
          <>
            <h1 className={styles.title}>What to install</h1>
            <p className={styles.lead}>One choice for the operation, then which products.</p>
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
      case 4:
        return (
          <>
            <h1 className={styles.title}>License files</h1>
            <p className={styles.lead}>
              Point to your Argent license file. Need one? Argent.com → Products and Support.
            </p>
            <label className={styles.label}>Node</label>
            <input className={styles.input} value={installNode} readOnly />
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
          </>
        );
      case 5:
        return (
          <>
            <h1 className={styles.title}>Folders</h1>
            <p className={styles.lead}>Where files land. Defaults match the current installer.</p>
            <label className={styles.label}>Node</label>
            <input className={styles.input} value={installNode} readOnly />
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
      case 6:
        return (
          <>
            <h1 className={styles.title}>Service &amp; SQL</h1>
            <p className={styles.lead}>Service account, then database — only what you need.</p>
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
                for production. help.Argent.com if you need an engineer.
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
          </>
        );
      case 7:
        return (
          <>
            <h1 className={styles.title}>Your details</h1>
            <p className={styles.lead}>Stored under Software\Argent\Customer — same as today.</p>
            <div className={styles.fieldGrid}>
              <div className={styles.fieldFull}>
                <label className={styles.label}>Email</label>
                <input
                  className={styles.input}
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                />
              </div>
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
          </>
        );
      case 8:
        return (
          <>
            <h1 className={styles.title}>Installing</h1>
            <p className={styles.lead}>{progressStatus}</p>
            <div className={styles.barTrack}>
              <div className={styles.barFill} style={{ width: `${progress}%` }} />
            </div>
            <p className={styles.muted}>{progress}%</p>
            <p className={styles.copy} style={{ marginTop: 16 }}>
              Still no extra downloads — Setup uses what was already bundled.
            </p>
          </>
        );
      default:
        return (
          <>
            <h1 className={styles.title}>You&apos;re set</h1>
            <p className={styles.lead}>Installed in about 266 seconds.</p>
            <ul className={styles.doneList}>
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
          <div className={styles.sidebarHead}>
            <ArgentBrand />
          </div>
          <div className={styles.sidebarScroll}>
            <div className={styles.setupLabel}>Setup wizard</div>
            <p className={styles.productLines}>
              Job Scheduler 10.0-2401-64W-A
              <br />
              Queue Engine 10.0-2401-64W-A
            </p>
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
