import Link from "next/link";

import type { Locale } from "@/shared/config/locales";
import { getHomeText } from "@/shared/i18n/home-text";
import { LocaleSwitch } from "@/features/locale-switch";

export type SiteHeaderProps = {
  lang: Locale;
};

export function SiteHeader({ lang }: SiteHeaderProps) {
  const texts = getHomeText(lang);

  return (
    <header className="flex flex-wrap items-center justify-between gap-3 border-b border-border pb-4">
      <h1 className="text-3xl font-semibold tracking-tight">
        <Link
          href={`/${lang}`}
          className="rounded-sm underline-offset-4 hover:underline focus-visible:ring-2 focus-visible:ring-foreground focus-visible:outline-none"
        >
          {texts.pageTitle}
        </Link>
      </h1>
      <LocaleSwitch lang={lang} />
    </header>
  );
}
