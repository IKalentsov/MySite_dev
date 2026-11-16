import type { Locale } from "@/shared/config/locales";
import { getHomeText } from "@/shared/i18n/home-text";

/**
 * The shape a project card renders. It is structural on purpose: the page passes
 * the API's `Project` and the compiler keeps the two in step at the call site,
 * while this slice never imports the API client (FSD: entities sit below `pages`).
 */
export type ProjectCardData = {
  title: string;
  summary: string;
  stack: readonly string[];
  link: string;
  year: number | string;
};

export type ProjectCardProps = {
  project: ProjectCardData;
  locale: Locale;
};

export function ProjectCard({ project, locale }: ProjectCardProps) {
  const texts = getHomeText(locale);

  return (
    <article className="flex h-full flex-col rounded border border-border p-4">
      <h3 className="text-lg font-semibold text-balance break-words">{project.title}</h3>
      <p className="mt-1 text-sm text-muted-foreground text-pretty break-words">{project.summary}</p>
      <ul className="mt-3 flex flex-wrap gap-1">
        {project.stack.map((technology) => (
          <li
            key={technology}
            className="rounded bg-secondary px-2 py-0.5 text-xs text-secondary-foreground"
          >
            {technology}
          </li>
        ))}
      </ul>
      <div className="mt-auto flex flex-wrap items-center justify-between gap-3 pt-3">
        <span className="text-sm text-muted-foreground">{project.year}</span>
        <a
          href={project.link}
          target="_blank"
          rel="noreferrer noopener"
          aria-label={`${texts.openProject}: ${project.title}`}
          className="inline-flex min-h-6 items-center rounded-sm text-sm text-primary underline underline-offset-4 hover:no-underline focus-visible:ring-2 focus-visible:ring-foreground focus-visible:outline-none"
        >
          {texts.openProject}
        </a>
      </div>
    </article>
  );
}
