import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Argent Job Scheduler Setup",
  description: "Modern Setup wizard UI prototype for Argent Job Scheduler",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
