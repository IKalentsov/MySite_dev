import type { Locale } from "@/shared/config/locales";
import { getHomeText } from "@/shared/i18n/home-text";

export type ProfileAboutProps = {
  profile: { headline: string; about: string };
  locale: Locale;
};

export function ProfileAbout({ profile, locale }: ProfileAboutProps) {
  const texts = getHomeText(locale);

  return (
    <section aria-labelledby="about-heading" className="mt-8">
      <h2 id="about-heading" className="text-xl font-semibold tracking-tight text-balance">
        {texts.aboutHeading}
      </h2>
      <p className="mt-3 max-w-[65ch] text-lg text-pretty break-words">{profile.headline}</p>
      <p className="mt-2 max-w-[65ch] text-muted-foreground text-pretty break-words">
        {profile.about}
      </p>
    </section>
  );
}
