# Free live demo (checklist)

I could not check current free-tier limits from the build environment, so verify each limit on the provider's pricing page before relying on it.

## 1. API (Demo mode, one Docker service)
Demo mode uses SQLite and runs the notifications inside the same process, so one container is enough.
- Host: any free Docker host (for example Render's free web service). Build file: `src/HireFlow.Api/Dockerfile`, build context: repository root.
- Environment variables:
  - `ASPNETCORE_ENVIRONMENT=Demo`
  - `Jwt__Key=<random text, 32+ characters>`
  - `Cors__Origins__0=https://<your-admin-site>`
  - `Cors__Origins__1=https://<your-careers-site>`
- Free hosts put the app to sleep when idle and the SQLite file is reset on redeploy. The demo data is re-created on start.
- The first request after sleeping can take about a minute. The Angular app shows a "Server is waking up" banner.

## 2. Angular staff app
```
cd web-admin
# edit src/environments/environment.ts: apiUrl = your live API address
npm install
npm run build
```
Upload `dist/web-admin/browser` to a free static host (Cloudflare Pages, GitHub Pages, Netlify).
Add a redirect so every path serves `index.html` (single-page app).

## 3. Next.js careers site
Set `NEXT_PUBLIC_API_URL` to the live API address, then deploy `web-careers` to a host that supports Next.js (for example Vercel or Cloudflare Pages).
Also change the `copyLink` address in `web-admin/src/app/pages/offers.component.ts` to your live careers site.

## 4. Before sending the link to HR
Open the live site on your phone with mobile data, wait for the server to wake, log in with the demo accounts, and run the full flow once.
