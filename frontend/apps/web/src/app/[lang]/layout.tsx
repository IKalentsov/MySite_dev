import type { Metadata } from "next";
import { notFound } from "next/navigation";
import type { ReactNode } from "react";

import { Providers } from "../providers";
import "../globals.css";

const locales = ["ru", "en"] as const;

type Locale = (typeof locales)[number];

function isLocale(value: string): value is Locale {
  return locales.some((locale) => locale === value);
}

export const metadata: Metadata = {
  title: "MySite",
};

export default async function LocaleLayout({
  children,
  params,
}: {
  children: ReactNode;
  params: Promise<{ lang: string }>;
}) {
  const { lang } = await params;

  if (!isLocale(lang)) {
    notFound();
  }

  return (
    <html lang={lang} suppressHydrationWarning>
      <body>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
