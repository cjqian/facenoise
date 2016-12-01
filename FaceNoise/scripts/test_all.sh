# this needs to be run from the main directory
# takes in one input, the degree of perturbation
echo "1. DETECTING/PERTURBING FACES."
for file in input/*.jpg; do
	basefile=`basename "$file"`
	./bin/Debug/FaceNoise.exe -e $basefile $1
done

echo "\n\n"
echo "2. RECGONIZING FACES."
python scripts/fb_endpoint.py
