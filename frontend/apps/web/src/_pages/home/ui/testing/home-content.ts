import type { HomeContent } from "@/_pages/home";

export const successContent = {
  profile: {
    locale: "ru",
    headline: "Full-stack .NET-разработчик",
    about: "Разрабатываю веб-приложения на .NET и React.",
  },
  projects: [
    {
      title: "MySite",
      summary: "Персональный сайт-портфолио",
      stack: [".NET", "React", "PostgreSQL"],
      link: "https://github.com/IKalentsov/MySite_dev",
      year: 2026,
    },
    {
      title: "DirectoryService",
      summary: "Сервис управления структурой предприятия",
      stack: ["C#", "EF Core"],
      link: "https://github.com/IKalentsov/SachkovTech/tree/master/DirectoryService",
      year: 2026,
    },
    {
      title: "Harness",
      summary: "База для настройки агентной среды",
      stack: ["Markdown", "Skills"],
      link: "https://github.com/IKalentsov/Harness",
      year: 2026,
    },
  ],
} satisfies HomeContent;

export const emptyContent = {
  profile: null,
  projects: [],
} satisfies HomeContent;

export const partialNoProfile = {
  profile: null,
  projects: [
    {
      title: "MySite",
      summary: "Персональный сайт-портфолио",
      stack: [".NET", "React"],
      link: "https://github.com/IKalentsov/MySite_dev",
      year: 2026,
    },
  ],
} satisfies HomeContent;

export const partialNoProjects = {
  profile: {
    locale: "ru",
    headline: "Full-stack .NET-разработчик",
    about: "Разрабатываю веб-приложения на .NET и React.",
  },
  projects: [],
} satisfies HomeContent;

const longWord = "a".repeat(400);

export const longDescriptionContent = {
  profile: {
    locale: "ru",
    headline: "Full-stack .NET-разработчик",
    about: `Краткий текст. ${longWord}`,
  },
  projects: [
    {
      title: "Проект с очень длинным описанием, которое обязано переноситься, а не обрезаться",
      summary: longWord,
      stack: [".NET"],
      link: "https://github.com/IKalentsov/MySite_dev",
      year: 2026,
    },
  ],
} satisfies HomeContent;

export const longWordFixture = longWord;
