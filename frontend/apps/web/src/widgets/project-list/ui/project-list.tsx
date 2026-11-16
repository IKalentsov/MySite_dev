import type { Locale } from "@/shared/config/locales";
import { getHomeText } from "@/shared/i18n/home-text";
import { ProjectCard, type ProjectCardData } from "@/entities/project";

export type ProjectListProps = {
  projects: readonly ProjectCardData[];
  locale: Locale;
};

export function ProjectList({ projects, locale }: ProjectListProps) {
  const texts = getHomeText(locale);

  return (
    <section aria-labelledby="projects-heading" className="mt-8">
      <h2 id="projects-heading" className="text-xl font-semibold tracking-tight text-balance">
        {texts.projectsHeading}
      </h2>
      <ul className="mt-4 grid gap-4 sm:grid-cols-2">
        {projects.map((project) => (
          <li key={project.link}>
            <ProjectCard project={project} locale={locale} />
          </li>
        ))}
      </ul>
    </section>
  );
}
