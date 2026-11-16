import type { NextConfig } from "next";

const redirects = [{ source: "/", destination: "/ru", permanent: true }];

const nextConfig: NextConfig = {
  redirects: () => Promise.resolve(redirects),
};

export default nextConfig;
