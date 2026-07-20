# Train Booking Automation

Windows desktop app that searches trains (via etrain.info) and semi-automates IRCTC booking with Playwright.

## Features

- Search trains by From / To / Date (etrain.info, headless Chromium)
- Book on IRCTC with a visible browser (class, quota, refresh loop)
- Passenger autofill (name, age, gender, berth, food)
- Payment method selection (BHIM/UPI, eWallet, cards/net banking)
- Confirm-berths-only and auto-upgrade preferences
- Optional scheduled search time for Tatkal (`HH:mm:ss`)
- IRCTC password encrypted at rest with Windows DPAPI

## Requirements

- Windows 10/11
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Chromium for Playwright (auto-installed on first search if missing)

## Run

```powershell
cd D:\github\train-automation-desktop
dotnet restore
dotnet run
```

Smoke-test etrain search only:

```powershell
dotnet run -- --smoke-test
```

## Typical Tatkal flow

1. **Passengers** — add travellers (name/age/gender/berth/food).
2. **IRCTC Settings** — username, password, mobile, payment method.
   - Set **Scheduled Search Time** e.g. `09:59:55` (AC Tatkal) or `10:59:55` (SL).
   - Raise **Availability timeout** (e.g. 180s) and lower refresh interval if needed.
3. **Search & Run** — pick stations, date, **Quota = Tatkal (TQ)**, preferred **Class**.
4. Search trains, select the train row, click **Book on IRCTC** *before* the scheduled time.
5. Enter OTP / captcha when prompted; complete UPI/QR payment when the QR appears.

## Manual steps (by design)

IRCTC requires human confirmation for:

- Login OTP or image captcha
- Review-page captcha (when shown)
- Final UPI / card / net-banking payment

## Config

Saved next to the exe as `config.json` (gitignored). Password field is DPAPI-protected (`dpapi:...`).

## Disclaimer

For personal / educational use. Comply with IRCTC terms of service. Do not use for bulk booking or resale.
