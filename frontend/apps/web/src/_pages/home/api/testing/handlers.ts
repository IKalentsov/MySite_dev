import { http, HttpResponse } from "msw";

import type { HomeResponse, OwnerProfile, Project } from "@mysite/api-client";

const profileFixture: OwnerProfile = {
  locale: "ru",
  headline: "Full-stack .NET-разработчик",
  about: "Разрабатываю веб-приложения на .NET и React.",
};

const projectFixture: Project = {
  title: "MySite",
  summary: "Персональный сайт-портфолио",
  stack: [".NET", "React", "PostgreSQL"],
  link: "https://github.com/IKalentsov/MySite_dev",
  year: 2026,
};

const homeFixture: HomeResponse = {
  profile: profileFixture,
  projects: {
    items: [projectFixture],
    nextCursor: null,
    hasMore: false,
  },
};

export const handlers = [
  http.get("http://localhost:5054/api/v1/home", () => HttpResponse.json(homeFixture)),
] as const;
