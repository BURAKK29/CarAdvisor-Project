import * as icons from "simple-icons/icons";
import fs from "fs";
import path from "path";

const outputDir = path.resolve("src/assets/logos");

if (!fs.existsSync(outputDir)) {
  fs.mkdirSync(outputDir, { recursive: true });
}

const brands = [
  "siAudi",
  "siBmw",
  "siCitroen",
  "siDacia",
  "siFiat",
  "siFord",
  "siHonda",
  "siHyundai",
  "siKia",
  "siMazda",
  "siMercedesbenz",
  "siNissan",
  "siOpel",
  "siPeugeot",
  "siRenault",
  "siSkoda",
  "siSuzuki",
  "siToyota",
  "siVolkswagen",
  "siVolvo"
];

brands.forEach((key) => {
  const icon = icons[key];
  if (!icon) {
    console.warn(`❌ Bulunamadı: ${key}`);
    return;
  }

  const svg = `
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
  <path fill="#${icon.hex}" d="${icon.path}" />
</svg>
`.trim();

  fs.writeFileSync(
    path.join(outputDir, `${icon.title.toLowerCase().replace(/\s+/g, "-")}.svg`),
    svg
  );

  console.log(`✅ ${icon.title}.svg oluşturuldu`);
});
