import Link from "next/link";

import type { Locale } from "@/shared/config/locales";
import { getHomeText } from "@/shared/i18n/home-text";

export type LocaleSwitchProps = {
  lang: Locale;
};

const baseLinkClassName =
  "inline-flex min-h-6 items-center rounded-sm text-sm underline-offset-4 focus-visible:ring-2 focus-visible:ring-foreground focus-visible:outline-none";

const inactiveLinkClassName = `${baseLinkClassName} text-muted-foreground underline hover:no-underline`;
const activeLinkClassName = `${baseLinkClassName} font-semibold text-foreground`;

/**
 * The locale lives in the URL, so the switch is two plain links: no client state,
 * no hydration and a result that survives a reload or a forwarded link.
 */
export function LocaleSwitch({ lang }: LocaleSwitchProps) {
  const texts = getHomeText(lang);

  return (
    <nav aria-label={texts.languageSwitch} className="flex items-center gap-3">
      <Link
        href="/ru"
        lang="ru"
        aria-current={lang === "ru" ? "page" : undefined}
        className={lang === "ru" ? activeLinkClassName : inactiveLinkClassName}
      >
        {texts.localeRu}
      </Link>
      <Link
        href="/en"
        lang="en"
        aria-current={lang === "en" ? "page" : undefined}
        className={lang === "en" ? activeLinkClassName : inactiveLinkClassName}
      >
        {texts.localeEn}
      </Link>
    </nav>
  );
}
