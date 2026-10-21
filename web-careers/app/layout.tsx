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
            <Link href="/" className="brand">HireFlow Careers</Link>
            <Link href="/">Jobs</Link>
            <Link href="/status">Check my application</Link>
          </div>
        </header>
        <main>{children}</main>
        <footer>HireFlow portfolio project</footer>
      </body>
    </html>
  );
}
