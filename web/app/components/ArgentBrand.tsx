import styles from "./ArgentBrand.module.css";

type Props = {
  compact?: boolean;
};

/** Transparent wordmark — no PNG black bar, no decorative glyphs. */
export function ArgentBrand({ compact }: Props) {
  return (
    <div className={compact ? styles.compact : styles.header} aria-label="Argent">
      <span className={styles.wordmarkText}>A R G E N T</span>
      <sup className={styles.reg}>®</sup>
    </div>
  );
}
