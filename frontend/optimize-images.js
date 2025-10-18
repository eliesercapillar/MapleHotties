import sharp from 'sharp';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const inputDir = path.join(__dirname, 'public/bgs');
const outputDir = path.join(__dirname, 'public/bgs/optimized');

fs.mkdirSync(outputDir, { recursive: true });

const files = fs.readdirSync(inputDir).filter(file => file.endsWith('.png'));

for (const file of files) {
  const inputPath = path.join(inputDir, file);
  const outputPath = path.join(outputDir, file.replace('.png', '.webp'));
  
  try {
    await sharp(inputPath)
      .resize(750, 1334, { fit: 'cover' })
      .webp({ quality: 85 })
      .toFile(outputPath);
    
    console.log(`Optimized ${file}`);
  } catch (error) {
    console.error(`Failed to optimize ${file}:`, error.message);
  }
}

console.log('\nEnd of optimization.');