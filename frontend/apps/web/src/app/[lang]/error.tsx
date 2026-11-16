"use client";

import { useParams } from "next/navigation";
import { useEffect } from "react";

import { Button } from "@mysite/ui";

import { DEFAULT_LOCALE, isLocale } from "@/shared/config/locales";
import { getHomeText } from "@/shared/i18n/home-text";

/**
 * The route's error boundary: a clear message and a retry that asks the server for
 * the page again. The technical detail goes to the log, never to the screen.
 */
export default function HomeError({ error, reset }: { error: Error; reset: () => void }) {
  const params = useParams<{ lang?: string }>();
  const lang = params?.lang && isLocale(params.lang) ? params.lang : DEFAULT_LOCALE;
  const texts = getHomeText(lang);

  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <main className="mx-auto flex min-h-svh w-full max-w-3xl flex-col items-center justify-center px-6 py-16 text-center">
      <h1 className="text-2xl font-semibold tracking-tight">{texts.errorTitle}</h1>
      <p className="mt-3 text-muted-foreground">{texts.errorMessage}</p>
      <Button onClick={() => reset()} className="mt-6">
        {texts.retryButton}
      </Button>
    </main>
  );
}
