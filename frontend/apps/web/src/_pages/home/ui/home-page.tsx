import { notFound } from "next/navigation";
import { isLocale } from "@/shared/config/locales";
import { getHomeContent } from "@/_pages/home/api/get-home-content";
import { HomePageView } from "./home-page-view";

export async function HomePage({
  params,
}: {
  params: Promise<{ lang: string }>;
}) {
  const { lang } = await params;

  if (!isLocale(lang)) {
    notFound();
  }

  const content = await getHomeContent(lang);

  return <HomePageView lang={lang} content={content} />;
}
