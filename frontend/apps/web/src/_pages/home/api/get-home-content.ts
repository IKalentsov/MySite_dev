import { getHome, type OwnerProfile, type Project } from "@mysite/api-client";

export type HomeContent = {
  profile: OwnerProfile | null;
  projects: Project[];
};

/**
 * The page's data boundary: the only place in the app that calls the API client.
 * A failed request throws `ApiError`, which the route's `error.tsx` handles.
 */
export async function getHomeContent(lang: string): Promise<HomeContent> {
  const { profile, projects } = await getHome(lang);

  return { profile, projects: projects.items };
}
