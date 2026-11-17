import type { Locale } from "@/shared/config/locales";
import { getHomeText } from "@/shared/i18n/home-text";
import { CONTACT_LINKS } from "@/shared/config/contact-links";

export type ContactLinksProps = {
  lang: Locale;
};

const linkClassName =
  "inline-flex min-h-6 items-center rounded-sm text-primary underline underline-offset-4 hover:no-underline focus-visible:ring-2 focus-visible:ring-foreground focus-visible:outline-none";

export function ContactLinks({ lang }: ContactLinksProps) {
  const texts = getHomeText(lang);

  return (
    <section aria-labelledby="contact-heading" className="mt-10 border-t border-border pt-4">
      <h2 id="contact-heading" className="text-xl font-semibold tracking-tight text-balance">
        {texts.contactHeading}
      </h2>
      <ul className="mt-3 flex flex-wrap gap-x-6 gap-y-2">
        <li>
          <a
            href={CONTACT_LINKS.telegram}
            target="_blank"
            rel="noreferrer noopener"
            className={linkClassName}
          >
            {texts.contactTelegramLabel}
          </a>
        </li>
        <li>
          <a href={CONTACT_LINKS.email} className={linkClassName}>
            {texts.contactEmailLabel}
          </a>
        </li>
      </ul>
    </section>
  );
}
