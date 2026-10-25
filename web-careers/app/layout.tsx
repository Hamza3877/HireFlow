import type { Metadata } from 'next';
import Link from 'next/link';
import './globals.css';

export const metadata: Metadata = {
  title: 'HireFlow Careers',
  description: 'Find your next job and apply in minutes.'
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <header className="site">
          <div>
            <Link href="/" className="brand">HireFlow<small>Careers</small></Link>
            <Link href="/" className="hf-nav hide-sm">Jobs</Link>
            <Link href="/status" className="hf-nav out">Check my application</Link>
          </div>
        </header>
        <main>{children}</main>
        <footer className="site">
          <div className="hf-fwrap">
            <div className="hf-fgrid">
              <div>
                <Link href="/" className="brand">HireFlow<small>Careers</small></Link>
                <p>A portfolio hiring platform covering jobs, applications, interviews, offers and onboarding in one flow.</p>
              </div>
              <div>
                <h4>Candidates</h4>
                <Link href="/">Open positions</Link>
                <Link href="/status">Check my application</Link>
              </div>
              <div>
                <h4>Built with</h4>
                <span>ASP.NET Core and SQL Server</span>
                <span>Angular and Next.js</span>
              </div>
            </div>
            <div className="hf-copy">HireFlow portfolio project</div>
          </div>
        </footer>
      </body>
    </html>
  );
}
