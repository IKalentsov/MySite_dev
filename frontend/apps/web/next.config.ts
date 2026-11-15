import type { NextConfig } from "next";

const redirects = [{ source: "/", destination: "/ru", permanent: false }];

const nextConfig: NextConfig = {
  redirects: () => Promise.resolve(redirects),
};

export default nextConfig;
