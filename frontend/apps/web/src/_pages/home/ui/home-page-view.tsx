import type { Locale } from "@/shared/config/locales";
import { getHomeText } from "@/shared/i18n/home-text";

import type { HomeContent } from "../api/get-home-content";
import { ContactLinks } from "@/widgets/contact-links";
import { ProfileAbout } from "@/widgets/profile-about";
import { ProjectList } from "@/widgets/project-list";
import { SiteHeader } from "@/widgets/site-header";

export type HomePageViewProps = {
  lang: Locale;
  content: HomeContent;
};

/**
 * The screen's synchronous view: everything it shows arrives as props, so the four
 * states are decided here and covered by component tests. Loading and error live in
 * the route files (`loading.tsx`, `error.tsx`).
 */
export function HomePageView({ lang, content }: HomePageViewProps) {
  const texts = getHomeText(lang);
  const profile = content.profile;
  const projects = content.projects;
  const hasProfile = profile !== null;
  const hasProjects = projects.length > 0;
  const isEmpty = !hasProfile && !hasProjects;
  const isPartial = !isEmpty && (!hasProfile || !hasProjects);

  return (
    <div className="mx-auto flex min-h-svh w-full max-w-3xl flex-col px-6 py-10">
      <SiteHeader lang={lang} />

      <main className="flex flex-col">
        {isPartial && <p className="mt-4 text-sm text-muted-foreground">{texts.partialLoadLabel}</p>}

        {isEmpty ? (
          <section aria-labelledby="empty-heading" className="mt-10">
            <h2 id="empty-heading" className="text-xl font-semibold tracking-tight text-balance">
              {texts.emptyStateTitle}
            </h2>
            <p className="mt-2 text-muted-foreground">{texts.emptyStateHint}</p>
          </section>
        ) : null}

        {isEmpty ? null : (
          <>
            {hasProfile ? (
              <ProfileAbout profile={profile} locale={lang} />
            ) : (
              <section aria-labelledby="about-heading" className="mt-8">
                <h2 id="about-heading" className="text-xl font-semibold tracking-tight text-balance">
                  {texts.aboutHeading}
                </h2>
                <p className="mt-3 text-muted-foreground">{texts.noProfileNote}</p>
              </section>
            )}

            {hasProjects ? (
              <ProjectList projects={projects} locale={lang} />
            ) : (
              <section aria-labelledby="projects-heading" className="mt-8">
                <h2 id="projects-heading" className="text-xl font-semibold tracking-tight text-balance">
                  {texts.projectsHeading}
                </h2>
                <p className="mt-3 text-muted-foreground">{texts.noProjectsNote}</p>
              </section>
            )}
          </>
        )}

        <ContactLinks lang={lang} />
      </main>
    </div>
  );
}
